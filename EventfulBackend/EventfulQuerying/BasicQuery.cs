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
using Eventful.Shared.MongoDB;

namespace EventfulBackend.EventfulQuerying
{
	public static class BasicQuery
	{
	
		public static BasicQueryResult ExecuteQuery(string eqlQuery, string startTime, string endTime, TimeZoneInfo resultsTimeZone, int pageSize, int requestedPageNumber)
		{

			DateTime startDT = EventfulQueryParser.ParseRequestedTime(startTime, resultsTimeZone);
			DateTime endDT = EventfulQueryParser.ParseRequestedTime(endTime, resultsTimeZone);

			return ExecuteQuery(eqlQuery, startDT, endDT, resultsTimeZone, pageSize, requestedPageNumber);

		}

		public static BasicQueryResult ExecuteQuery(string eqlQuery, DateTime startTime, DateTime endTime, TimeZoneInfo resultsTimeZone, int pageSize, int requestedPageNumber)
		{
			eqlQuery = FormatEQLQuery(eqlQuery);
			IMongoQuery criteriaQuery = EventfulQueryParser.ParseSearchStringToIMongoQuery(eqlQuery);
			if (!string.IsNullOrWhiteSpace(eqlQuery) && criteriaQuery == null)
			{
				throw new ApplicationException(string.Format("Unable to parse EQL Query: {0}", eqlQuery));
			}

			IMongoQuery timestamps = Query.And(Query.LTE("Timestamp", endTime), Query.GTE("Timestamp", startTime));

			IMongoQuery fullQuery = timestamps;
			if (criteriaQuery != null)
			{
				fullQuery = Query.And(criteriaQuery, timestamps);
			}

			BasicQueryResult retVal = new BasicQueryResult
			{
				EQLQuery = eqlQuery,
				EndTime = endTime,
				StartTime = startTime,
				QueryDocument = fullQuery,
				CurrentPage = requestedPageNumber,
				PageSize = pageSize
			};

			EventfulDBManager.ExecuteInContext((db) =>
			{
				int skip = 0;
				int take = 500; 
				if (pageSize > 0 && requestedPageNumber > 0)
				{
					skip = (requestedPageNumber - 1) * pageSize;
					take = pageSize;
				}

				var results = db.GetCollection("eventfulEvents").Find(fullQuery).SetSortOrder(SortBy.Descending("Timestamp")).SetSkip(skip).SetLimit(take);

				retVal.TotalResultCount = db.GetCollection("eventfulEvents").Find(fullQuery).Count();

				List<dynamic> queryResults = convertBsonDocumentsToSerializable(results, resultsTimeZone);

				retVal.QueryResults = queryResults;
			});

			return retVal;
		}

		public static string FormatEQLQuery(string eqlQuery)
		{
			if (string.IsNullOrEmpty(eqlQuery))
			{
				return string.Empty;
			}
			return eqlQuery.Replace(" &&", "\r\n&&").Replace(" ||", "\r\n||");
		}

		/// <summary>
		/// Converts BsonDocuments to dynamic objects with serialization-safe ObjectIDs and BsonDateTimes converted to local date times.
		/// </summary>
		/// <param name="results"></param>
		/// <returns></returns>
		private static List<dynamic> convertBsonDocumentsToSerializable(MongoCursor<BsonDocument> results, TimeZoneInfo resultsTimeZone)
		{
			List<dynamic> queryResults = new List<dynamic>();
			foreach (BsonDocument bd in results)
			{
				dynamic d = new ExpandoObject();
				foreach (BsonElement be in bd)
				{
					string val = be.Value.ToString();
					if (be.Name == "_id")
					{
						val = ((BsonObjectId)be.Value).AsObjectId.ToString();
					}
					else if (be.Value.IsValidDateTime)
					{
						val = TimeZoneUtils.ToUserLocalTime(be.Value.ToLocalTime(), TimeZoneUtils.CST).ToString();
					}
					((IDictionary<string, object>)d)[be.Name] = val;
				}
				((List<dynamic>)queryResults).Add(d);
			}
			return queryResults;
		}
	}
}
