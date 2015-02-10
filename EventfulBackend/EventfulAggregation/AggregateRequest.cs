using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace eventfulBackend.eventfulAggregation
{
	public class AggregateRequest
	{
		public string GroupByFieldName { get; set; }
		public string RequestedStartTime { get; set; }
		public string RequestedEndTime { get; set; }
		public string EqlQuery { get; set; }
		public string Title { get; set; }
		public string AggregateOperator { get; set; }
		public string AggregateFieldName { get; set; }
		public int MaxResultsBeforeOutliers { get; set; }
	}
}
