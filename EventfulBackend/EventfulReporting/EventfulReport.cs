﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EventfulBackend.EventfulAggregation;
using EventfulBackend.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.GridFS;
using MongoDB.Driver.Linq;
using NLog;
using System.Web;

namespace EventfulBackend.EventfulReporting
{
	public class EventfulReport : MongoDBEntity
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
	
		public List<AggregateRequest> AggregateRequests { get; set; }
		public string Title { get; set; }

		public void Insert()
		{
			EventfulDBManager.ExecuteInContext((db) =>
			{
				var dbReports = getReportsCollection(db);
				dbReports.Insert(this);
			});
		}

		private static MongoCollection<EventfulReport> getReportsCollection(MongoDatabase db)
		{
			return db.GetCollection<EventfulReport>("eventfulReports");
		}

		public static IEnumerable<EventfulReport> GetAllReports()
		{
			IEnumerable<EventfulReport> retVal = null;
			EventfulDBManager.ExecuteInContext((db) =>
			{
				var dbReports = getReportsCollection(db);
				retVal = dbReports.FindAll().ToArray();
			});

			return retVal;
		}

		public static EventfulReport GetById(string id)
		{
			EventfulReport er = null;
			EventfulDBManager.ExecuteInContext((db) =>
			{
				var dbReports = getReportsCollection(db);
				var query = getById(dbReports, id);

				er = query.FirstOrDefault();
			});

			return er;
		}

		private static IQueryable<EventfulReport> getById(MongoCollection<EventfulReport> dbReports, string id)
		{
			var query = from r in dbReports.AsQueryable()
						where r.Id == id
						select r;
			return query;
		}

		public static void DeleteById(string id)
		{
			EventfulDBManager.ExecuteInContext((db) =>
			{
				var dbReports = getReportsCollection(db);
				var query = getById(dbReports, id);
				dbReports.Remove(((MongoQueryable<EventfulReport>)query).GetMongoQuery());
			});
		}

		public void Update()
		{
			EventfulDBManager.ExecuteInContext((db) =>
			{
				var dbReports = getReportsCollection(db);
				dbReports.Save(this);
			});
		}

		public string GetAsHTMLTables(DateTime startTime, DateTime endTime)
		{
			StringBuilder sb = new StringBuilder();
			foreach (var req in AggregateRequests)
			{
				int i = 0;
				var result = BasicAggregator.RunAggregation(req.GroupByFieldName, startTime, endTime, TimeZoneInfo.Local, req.EqlQuery, req.AggregateOperator, req.AggregateFieldName, 5);
				//logger.Debug("EQL:\r\n{0}", result.EQLQuery);
				//logger.Debug("Mongo:\r\n{0}", result.MatchBsonDocument.ToString());

				sb.AppendLine(string.Format("<h3 style='margin: 20px 0 0 0; font-size: 14px; color: #3c709a;'>{0}</h3>", req.Title));
				foreach (var line in result.QueryResults)
				{
					sb.AppendFormat("<div style='padding-bottom: 3px; background-color: #{0};'>", (i % 2 == 0) ? "fff" : "eee");
					sb.AppendFormat("<span style='display: inline-block; width: 90%; text-align: left;'><b>{0}</b></span> <span>{1}</span>", HttpUtility.HtmlEncode(line.GroupName), line.AggregateValue);
					sb.AppendLine("</div>");
					i++;
				}
			}

			return sb.ToString();
		}

		public string GetAsSlackMessage(DateTime startTime, DateTime endTime)
		{
			StringBuilder sb = new StringBuilder();
			foreach (var req in AggregateRequests)
			{
				//int i = 0;
				var result = BasicAggregator.RunAggregation(req.GroupByFieldName, startTime, endTime, TimeZoneInfo.Local, req.EqlQuery, req.AggregateOperator, req.AggregateFieldName, 5);
				//logger.Debug("EQL:\r\n{0}", result.EQLQuery);
				//logger.Debug("Mongo:\r\n{0}", result.MatchBsonDocument.ToString());

				sb.AppendLine(string.Format("*{0}*", req.Title));
				sb.AppendLine();
				foreach (var line in result.QueryResults)
				{
					//sb.AppendFormat("<div style='padding-bottom: 3px; background-color: #{0};'>", (i % 2 == 0) ? "fff" : "eee");
					sb.AppendFormat("*{0}* {1}", HttpUtility.HtmlEncode(line.GroupName), line.AggregateValue);
					sb.AppendLine();
					//i++;
				}
				sb.AppendLine();
				sb.AppendLine();
			}

			return sb.ToString();
		}
	}
}
