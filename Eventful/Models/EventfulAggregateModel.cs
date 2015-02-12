using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EventfulBackend.EventfulAggregation;
using EventfulBackend.Utils;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using Eventful.Shared.MongoDB;

namespace Eventful.Models
{
	public class EventfulAggregateModel
	{
		public string Match { get; set; }
		public string Group { get; set; }
		public string Project { get; set; }
		public string RequestedStartTime { get; set; }
		public string RequestedEndTime { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public IEnumerable<dynamic> QueryResults { get; set; }
		public string EqlQuery { get; set; }
		public string GroupByFieldName { get; set; }
		public string AggregateOperator { get; set; }
		public string AggregateFieldName { get; set; }
		public int MaxResultsBeforeOutliers { get; set; }
		public string Title { get; set; }
		public string GroupName { get; set; }

		public EventfulAggregateModel()
		{
			RequestedStartTime = "now - 15 minutes";
			RequestedEndTime = "now";
			EqlQuery = "LogLevel == Error && eventfulGroup in (Web_Admin_Backend,Web_Main_Backend,Web_DataSite_Backend,Offline,Core.Service,Web_Divot_Backend)";
			GroupByFieldName = "ExceptionType";
			AggregateOperator = "Count";
			AggregateFieldName = "_id";
		}

		public void ExecuteAggregate()
		{
			AggregationResult result = BasicAggregator.RunAggregation(GroupByFieldName, RequestedStartTime, RequestedEndTime, TimeZoneUtils.CST, EqlQuery, AggregateOperator, AggregateFieldName, MaxResultsBeforeOutliers);
			Match = result.MatchBsonDocument.ToString();
			Group = result.GroupBsonDocument.ToString();
			Project = result.ProjectBsonDocument.ToString();
			StartTime = result.StartTime;
			EndTime = result.EndTime;
			QueryResults = result.QueryResults;
			EqlQuery = result.EQLQuery;
		}
	}
}