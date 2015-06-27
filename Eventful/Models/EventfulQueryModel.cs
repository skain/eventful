using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using NLog;
using Eventful.Shared.MongoDB;
using EventfulLogger.LoggingUtils;
using MongoDB.Driver.Builders;
using System.Text.RegularExpressions;
using System.Dynamic;
using EventfulBackend.EventfulQuerying;
using EventfulBackend.Utils;

namespace Eventful.Models
{
	public class EventfulQueryModel
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public string EqlQuery { get; set; }
		public string RequestedStartTime { get; set; }
		public string RequestedEndTime { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public IEnumerable<dynamic> QueryResults { get; set; }
		public string QuerySent { get; set; }
		public int PageSize { get; set; }
		public int CurrentPage { get; set; }
		public int RequestedPage { get; set; }
		public long TotalResultCount { get; set; }
		public string ShareUrl { get; set; }

		public EventfulQueryModel()
		{
			RequestedEndTime = "Now";
			RequestedStartTime = "Now - 2 days";
			EqlQuery = "";
			QueryResults = null;
			PageSize = 20;
			RequestedPage = 1;
			TotalResultCount = 0;
			ShareUrl = buildShortenedUrl();
		}

		public void ExecuteQuery()
		{
			BasicQueryResult result = BasicQuery.ExecuteQuery(EqlQuery, RequestedStartTime, RequestedEndTime, TimeZoneUtils.CST, PageSize, RequestedPage);

			this.QueryResults = result.QueryResults;
			this.QuerySent = result.QueryDocument.ToString();
			this.StartTime = result.StartTime;
			this.EndTime = result.EndTime;
			this.CurrentPage = result.CurrentPage;
			this.PageSize = result.PageSize;
			this.TotalResultCount = result.TotalResultCount;
			this.EqlQuery = result.EQLQuery; ShareUrl = buildShortenedUrl();
		}

		private string buildShortenedUrl()
		{
			var urlParams = new Dictionary<string, string>() {
				{ "RequestedStartTime", RequestedStartTime },
				{ "RequestedEndTime", RequestedEndTime },
				{ "EqlQuery", EqlQuery },
				{ "RequestedPage", RequestedPage.ToString() }
			};
			return EventfulUtils.BuildShortenedUrl(urlParams);
		}
	}
}