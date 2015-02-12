using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventfulBackend.QueryParsing;
using EventfulBackend.Utils;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NLog;
using Eventful.Shared.MongoDB;
using EventfulLogger.LoggingUtils;

namespace EventfulBackend.EventfulAggregation
{
	public static class BasicAggregator
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public static AggregationResult RunAggregation(string groupByFieldName, string startTime, string endTime, TimeZoneInfo resultsTimeZone, string eqlQuery, string aggregateOperator = "count", string aggregateFieldName = null, int maxResultsBeforeOutliers = 0)
		{
			DateTime startDT = EventfulQueryParser.ParseRequestedTime(startTime, resultsTimeZone);
			DateTime endDT = EventfulQueryParser.ParseRequestedTime(endTime, resultsTimeZone);

			return RunAggregation(groupByFieldName, startDT, endDT, resultsTimeZone, eqlQuery, aggregateOperator, aggregateFieldName, maxResultsBeforeOutliers);
		}

		public static AggregationResult RunAggregation(string groupByFieldName, DateTime startTime, DateTime endTime, TimeZoneInfo resultsTimeZone, string eqlQuery, string aggregateOperator, string aggregateFieldName, int maxResultsBeforeOutliers)
		{
			eqlQuery = EventfulBackend.EventfulQuerying.BasicQuery.FormatEQLQuery(eqlQuery);
			IMongoQuery criteriaQuery = EventfulQueryParser.ParseSearchStringToIMongoQuery(eqlQuery);


			var group = buildGroupingBsonDocument(groupByFieldName, aggregateOperator, aggregateFieldName);
			string groupNameValue = string.IsNullOrEmpty(groupByFieldName) ? "$_id" : string.Concat("$_id.", groupByFieldName);
			var project = new BsonDocument 
				{ 
					{ 
						"$project", 
						new BsonDocument 
							{ 
								{ "_id", 0 }, 
								{ "GroupName", groupNameValue }, 
								{ "Aggregate", 1 }, 
							} 
					} 
				};


			var matchQuery = Query.And(new[]{
				Query.LTE("Timestamp", endTime),				
				Query.GTE("Timestamp", startTime)
			});

			if (criteriaQuery != null)
			{
				matchQuery = Query.And(new[] {
					matchQuery,
					criteriaQuery
				});
			}

			var match = new BsonDocument
			{
				{
					"$match",
					matchQuery.ToBsonDocument()
				}
			};

			AggregationResult retVal = new AggregationResult
			{
				GroupBsonDocument = group,
				ProjectBsonDocument = project,
				MatchBsonDocument = match,
				StartTime = startTime,
				EndTime = endTime,
				GroupBy = groupByFieldName,
				EQLQuery = eqlQuery
			};

			try
			{
				EventfulDBManager.ExecuteInContext((db) =>
				{
					var result = db.GetCollection("eventfulEvents").Aggregate(match, group, project);
					var queryResults = result.ResultDocuments.Select(bd => new SingleAggregateResult { GroupName = getGroupNameFromBsonDoc(bd), AggregateValue = bd["Aggregate"].ToInt32() });
					/*
					 * This code block is the same as the select above, but makes debugging easier if there is a problem in the select
					IEnumerable<SingleAggregateResult> queryResults = new List<SingleAggregateResult>();
					foreach (BsonDocument bd in result.ResultDocuments)
					{
						SingleAggregateResult sar = new SingleAggregateResult();
						if (bd.Contains("GroupName") && !bd["GroupName"].IsBsonNull)
						{
							sar.GroupName = bd["GroupName"].AsString;
						}
						else
						{
							sar.GroupName = "NULL";
						}

						sar.AggregateValue = bd["Aggregate"].ToInt32();
						((List<SingleAggregateResult>)queryResults).Add(sar);
					}
					 * */
					retVal.QueryResults = queryResults.ToArray().OrderByDescending(qr => qr.AggregateValue);
				});
			}
			catch (Exception e)
			{
				logger.EventfulError(e, "Error executing Aggregate query with following params:\r\ngroup: {0}\r\nmatch: {1}\r\nproject: {2}", group, match, project);
				throw;
			}

			processOutliers(retVal, maxResultsBeforeOutliers);
			return retVal;
		}

		private static string getGroupNameFromBsonDoc(BsonDocument bd)
		{
			string groupName = "NULL";
			if (bd.Contains("GroupName") && !bd["GroupName"].IsBsonNull)
			{
				//Will need to add more conditionals for different field value types as they come up.
				if (bd["GroupName"].IsString)
				{
					groupName = bd["GroupName"].AsString;
				}
				else if (bd["GroupName"].IsInt64)
				{
					groupName = bd["GroupName"].AsInt64.ToString();
				}
			}

			return groupName;
		}

		private static void processOutliers(AggregationResult retVal, int maxResultsBeforeOutliers)
		{
			if (maxResultsBeforeOutliers < 1 || retVal.QueryResults.Count() < maxResultsBeforeOutliers + 2)
			{
				return;
			}

			var outliers = retVal.QueryResults.OrderByDescending(qr => qr.AggregateValue).Skip(maxResultsBeforeOutliers);
			var outliersSum = outliers.Sum(qr => qr.AggregateValue);

			var newResults = new List<SingleAggregateResult>(retVal.QueryResults.Take(maxResultsBeforeOutliers));

			newResults.Add(new SingleAggregateResult { GroupName = string.Format("Other ({0})", outliers.Count()), AggregateValue = outliersSum });

			retVal.QueryResults = newResults;
		}

		private static BsonDocument buildGroupingBsonDocument(string groupByFieldName, string aggregateOperator, string aggregateFieldName)
		{
			aggregateOperator = aggregateOperator.ToLower();
			BsonElement aggregateElement = null;
			//BsonDocument groupByDoc = string.IsNullOrEmpty(groupByFieldName) ? null : new BsonDocument { { groupByFieldName, string.Concat("$", groupByFieldName) } };

			switch (aggregateOperator)
			{
				case "first":
				case "last":
				case "max":
				case "min":
				case "avg":
				case "sum":
					aggregateOperator = string.Concat("$", aggregateOperator);
					aggregateElement = new BsonElement("Aggregate", new BsonDocument { { aggregateOperator, string.Concat("$", aggregateFieldName) } });
					break;
				case "count":
					aggregateOperator = "$sum";
					aggregateElement = new BsonElement("Aggregate", new BsonDocument { { aggregateOperator, 1 } });
					break;
				default:
					throw new ApplicationException(string.Format("Error. Unrecognized aggregate operator: '{0}'", aggregateOperator));
			}

			BsonElement groupByElement = null;
			if (string.IsNullOrEmpty(groupByFieldName))
			{
				groupByElement = new BsonElement("_id", "Aggregate");
			}
			else
			{
				groupByElement = new BsonElement("_id", new BsonDocument { { groupByFieldName, string.Concat("$", groupByFieldName) } });
			}

			var group = new BsonDocument 
				{ 
					{ "$group", 
						new BsonDocument
						{
							groupByElement,
							aggregateElement
						}
					} 
				};

			return group;
		}

	}
}
