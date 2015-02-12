using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using EventfulBackend.QueryParsing;
using EventfulBackend.Utils;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NLog;
using EventfulLogger.LoggingUtils;

namespace EventfulBackend.EventfulAggregation
{
	public class TimeSeriesAggregator
	{
		/*
		 {
        "$group" : {
                "_id" : {
                        "ExceptionType" : "$ExceptionType",
                        "Minute" : {
                                "$minute" : "$Timestamp"
                        }
                },
                "Aggregate" : {
                        "$sum" : 1
                }
			}
		}
		  
		 {
        "$project" : {
                "_id" : 0,
                "ExceptionType" : "$_id.ExceptionType",
                "Minute" : "$_id.Minute",
                "Aggregate" : 1
			}
		}
		 
		 */

		private static Logger logger = LogManager.GetCurrentClassLogger();

		//public static dynamic RunAggregation(/*string groupByFieldName, DateTime startTime, DateTime endTime, string eqlQuery, string aggregateOperator, string aggregateFieldName */)
		public static dynamic RunAggregation(string startTimeStr, string endTimeStr, TimeZoneInfo resultsTimeZone)
		{
			string eqlQuery = "LogLevel == Error";
			DateTime startTime = EventfulQueryParser.ParseRequestedTime(startTimeStr, resultsTimeZone);
			DateTime endTime = EventfulQueryParser.ParseRequestedTime(endTimeStr, resultsTimeZone);
			string aggregateOperator = "Count";
			string aggregateFieldName = null;
			string groupByFieldName = "ExceptionType";
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
								{ "Minute", "$_id.Minute" },
                                { "Aggregate", 1 }
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

			dynamic retVal = new ExpandoObject();
			retVal.GroupBsonDocument = group.ToString();
			retVal.ProjectBsonDocument = project.ToString();
			retVal.MatchBsonDocument = match.ToString();
			retVal.StartTime = startTime;
			retVal.EndTime = endTime;
			retVal.GroupBy = groupByFieldName;
			retVal.EQLQuery = eqlQuery.ToString();


			//AggregationResult retVal = new AggregationResult
			//{
			//	GroupBsonDocument = group,
			//	ProjectBsonDocument = project,
			//	MatchBsonDocument = match,
			//	StartTime = startTime,
			//	EndTime = endTime,
			//	GroupBy = groupByFieldName,
			//	EQLQuery = eqlQuery
			//};

			try
			{
				EventfulDBManager.ExecuteInContext((db) =>
				{
					var result = db.GetCollection("eventfulEvents").Aggregate(match, group, project);
					//var queryResults = result.ResultDocuments.Select(bd => new { GroupName = bd.Contains("GroupName") ? bd["GroupName"].AsString : "NULL", Count = bd["Aggregate"].AsInt32 });
					var queryResults = result.ResultDocuments.Select(bd => new { GroupName = bd.Contains("GroupName") ? bd["GroupName"].AsString : "NULL", Minute = bd["Minute"].ToInt32(), AggregateValue = bd["Aggregate"].ToInt32() });

					retVal.QueryResults = queryResults.ToArray();
				});
			}
			catch (Exception e)
			{
				logger.EventfulError(e, "Error executing Aggregate query with following params:\r\ngroup: {0}\r\nmatch: {1}\r\nproject: {2}", group, match, project);
				throw;
			}

			return retVal;
		}
		/*
		  {
        "$group" : {
                "_id" : {
                        "ExceptionType" : "$ExceptionType",
                        "Minute" : {
                                "$minute" : "$Timestamp"
                        }
                },
                "Aggregate" : {
                        "$sum" : 1
                }
			}
		}
		 */
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
			BsonElement timespanElement = new BsonElement("Minute", new BsonDocument { { "$minute", "$Timestamp" } });

			BsonElement groupByElement = null;
			if (string.IsNullOrEmpty(groupByFieldName))
			{
				groupByElement = new BsonElement("_id", new BsonDocument { { timespanElement } });
			}
			else
			{
				groupByElement = new BsonElement("_id", new BsonDocument { { groupByFieldName, string.Concat("$", groupByFieldName) }, timespanElement });
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
