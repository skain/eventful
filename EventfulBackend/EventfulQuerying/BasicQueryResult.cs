using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EventfulBackend.EventfulQuerying
{
	public class BasicQueryResult
	{
		public string EQLQuery { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public IEnumerable<dynamic> QueryResults { get; set; }
		public IMongoQuery QueryDocument { get; set; }
		public long TotalResultCount { get; set; }
		public int CurrentPage { get; set; }
		public int PageSize { get; set; }
	}
}
