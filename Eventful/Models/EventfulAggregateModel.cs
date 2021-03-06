﻿using System;
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
		public string ShareUrl { get; set; }

		public EventfulAggregateModel()
		{
			RequestedStartTime = "now - 15 minutes";
			RequestedEndTime = "now";
			EqlQuery = "";
			GroupByFieldName = "ExceptionType";
			AggregateOperator = "Count";
			AggregateFieldName = "_id";
			ShareUrl = buildShortenedUrl();
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
			EqlQuery = result.EQLQuery;ShareUrl = buildShortenedUrl();
		}

		private string buildShortenedUrl()
		{
			var urlParams = new Dictionary<string, string>() {
				{ "RequestedStartTime", RequestedStartTime },
				{ "RequestedEndTime", RequestedEndTime },
				{ "EqlQuery", EqlQuery },
				{ "GroupByFieldName", GroupByFieldName },
				{ "AggregateOperator", AggregateOperator },
				{ "AggregateFieldName", AggregateFieldName },
				{ "MaxResultsBeforeOutliers", MaxResultsBeforeOutliers.ToString() }
			};
			return EventfulUtils.BuildShortenedUrl(urlParams);
		}
	}
}