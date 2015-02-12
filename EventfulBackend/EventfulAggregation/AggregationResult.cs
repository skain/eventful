using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace EventfulBackend.EventfulAggregation
{
	public class AggregationResult
	{
		public BsonDocument GroupBsonDocument { get; set; }
		public BsonDocument ProjectBsonDocument { get; set; }
		public BsonDocument MatchBsonDocument { get; set; }
		public IEnumerable<SingleAggregateResult> QueryResults { get; set; }
		public string EQLQuery { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public string GroupBy { get; set; }
	}
}
