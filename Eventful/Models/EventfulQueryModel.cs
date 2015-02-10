using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using NLog;
using eventful.Shared.MongoDB;
using EventfulLogger.LoggingUtils;
using MongoDB.Driver.Builders;
using System.Text.RegularExpressions;
using System.Dynamic;
using eventfulBackend.eventfulQuerying;
using eventfulBackend.Utils;

namespace eventful.Models
{
	public class eventfulQueryModel
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

		public eventfulQueryModel()
		{
			RequestedEndTime = "Now";
			RequestedStartTime = "Now - 2 days";
			EqlQuery = "LogLevel == Error && eventfulGroup in (Web_Admin_Backend,Web_Main_Backend,Web_DataSite_Backend,Offline,Core.Service,Web_Divot_Backend)";
			QueryResults = null;
			PageSize = 20;
			RequestedPage = 1;
			TotalResultCount = 0;
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
			this.EqlQuery = result.EQLQuery;
		}
	}
}