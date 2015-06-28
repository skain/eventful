using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EventfulBackend.EventfulAggregation;
using EventfulBackend.EventfulReporting;
using EventfulBackend.QueryParsing;
using EventfulBackend.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace EventfulBackend.Klaxon
{
	public class Klaxon : MongoDBEntity
	{
		public string Name { get; set; }
		public string EqlQuery { get; set; }
		public string ComparisonOperator { get; set; }
		public int ComparisonThreshold { get; set; }
		public List<string> SubscriberEmails { get; private set; }
		public string CheckEvery { get; set; }
		[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
		public DateTime LastCheckedAt { get; set; }
		[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
		public DateTime NextCheckAt { get; set; }
		public long? NumMatched { get; private set; }
		public string FieldToAggregate { get; set; }
		public string AggregateOperation { get; set; }
		public string StartTime { get; set; }
		public string EndTime { get; set; }
		public AggregationResult AggregationResult { get; private set; }
		public bool TestResult { get; private set; }
		public string ReportId { get; set; }

		public Klaxon()
		{
			NumMatched = null;
			LastCheckedAt = new DateTime(1970, 1, 1);
			NextCheckAt = LastCheckedAt;
			SubscriberEmails = new List<string>();
			FieldToAggregate = "_id";
			AggregateOperation = "Count";

			//debug
			Name = "Test";
			EqlQuery = "LogLevel == Error";
			ComparisonOperator = ">";
			ComparisonThreshold = 10;
			CheckEvery = "15 minutes";
			StartTime = "Now - 15 minutes";
			EndTime = "Now";
		}

		public void Insert()
		{
			EventfulDBManager.ExecuteInContext((db) =>
			{
				var Klaxons = getCollection(db);
				Klaxons.Insert(this);
			});
		}

		public void Update()
		{
			EventfulDBManager.ExecuteInContext((db) =>
			{
				var Klaxons = getCollection(db);
				Klaxons.Save(this);
			});
		}

		//public IEnumerable<eventfulUser> GetSubscribers()
		//{
		//	return eventfulUser.GetByIds(SubscriberEmails);
		//}

		public void RunTest(bool updateDBWithResults)
		{
			var aggregationResult = BasicAggregator.RunAggregation(null, StartTime, EndTime, TimeZoneInfo.Local, EqlQuery, AggregateOperation, FieldToAggregate);
			if (aggregationResult.QueryResults == null || aggregationResult.QueryResults.Count() < 1)
			{
				this.NumMatched = 0;
			}
			else
			{
				this.NumMatched = aggregationResult.QueryResults.First().AggregateValue;
			}

			this.LastCheckedAt = DateTime.Now;
			string time = string.Concat("NOW + ", CheckEvery);
			this.NextCheckAt = EventfulQueryParser.ParseRequestedTime(time, TimeZoneInfo.Local);
			if (updateDBWithResults)
			{
				EventfulDBManager.ExecuteInContext((db) =>
				{
					var Klaxons = getCollection(db);
					Klaxons.Save(this);

				});
			}

			TestResult = testIfThresholdIsMet();
			this.AggregationResult = aggregationResult;
		}

		private bool testIfThresholdIsMet()
		{
			if (!NumMatched.HasValue)
			{
				return false;
			}

			switch (ComparisonOperator)
			{
				case "==":
					return NumMatched.Value == ComparisonThreshold;
				case "!=":
					return NumMatched.Value != ComparisonThreshold;
				case ">=":
					return NumMatched.Value >= ComparisonThreshold;
				case "<=":
					return NumMatched.Value <= ComparisonThreshold;
				case "<":
					return NumMatched.Value < ComparisonThreshold;
				case ">":
					return NumMatched.Value > ComparisonThreshold;
				default:
					throw new ApplicationException(string.Format("Error. Unrecognized ComparisonOperator: '{0}'", ComparisonThreshold));
			}
		}

		public static Klaxon GetById(string id)
		{
			Klaxon k = null;
			EventfulDBManager.ExecuteInContext((db) =>
			{
				var Klaxons = getCollection(db);
				var query = getById(Klaxons, id);

				k = query.FirstOrDefault();
			});

			k.TestResult = !k.testIfThresholdIsMet();
			return k;
		}

		private static IQueryable<Klaxon> getById(MongoCollection<Klaxon> Klaxons, string id)
		{
			var query = from i in Klaxons.AsQueryable()
						where i.Id == id
						select i;
			return query;
		}

		public static void DeleteById(string id)
		{
			EventfulDBManager.ExecuteInContext((db) =>
			{
				var Klaxons = getCollection(db);
				var query = getById(Klaxons, id);
				Klaxons.Remove(((MongoQueryable<Klaxon>)query).GetMongoQuery());
			});
		}

		public static IEnumerable<Klaxon> GetAllDueForTesting()
		{
			IEnumerable<Klaxon> ks = null;
			EventfulDBManager.ExecuteInContext((db) =>
			{
				var Klaxons = getCollection(db);
				var query = from k in Klaxons.AsQueryable()
							where k.NextCheckAt <= DateTime.Now
							select k;

				ks = query.ToArray();
			});

			return ks;
		}

		private static MongoCollection<Klaxon> getCollection(MongoDatabase db)
		{
			return db.GetCollection<Klaxon>("Klaxons");
		}

		public static IEnumerable<Klaxon> GetAll()
		{
			IEnumerable<Klaxon> ks = null;
			EventfulDBManager.ExecuteInContext((db) =>
			{
				var Klaxons = getCollection(db);
				ks = Klaxons.FindAll().ToArray();
			});

			setAllTestResults(ks);
			return ks;
		}

		private static void setAllTestResults(IEnumerable<Klaxon> ks)
		{
			foreach (var k in ks)
			{
				k.TestResult = !k.testIfThresholdIsMet();
			}
		}

		public EventfulReport GetReport()
		{
			if (string.IsNullOrWhiteSpace(ReportId))
			{
				return null;
			}

			return EventfulReport.GetById(ReportId);
		}

		public static void UpdateSubscriberEmail(string oldEmail, string newEmail)
		{
			EventfulDBManager.ExecuteInContext((db) =>
			{
				var Klaxons = getCollection(db);
				var query = getKlaxonsBySubscriberEmail(oldEmail, Klaxons);

				foreach (var k in query)
				{
					k.SubscriberEmails.Remove(oldEmail);
					k.SubscriberEmails.Add(newEmail);
					Klaxons.Save(k);
				}
			});
		}

		private static IQueryable<Klaxon> getKlaxonsBySubscriberEmail(string oldEmail, MongoCollection<Klaxon> Klaxons)
		{
			var query = from k in Klaxons.AsQueryable()
						where k.SubscriberEmails.Contains(oldEmail)
						select k;
			return query;
		}

		public static void DeleteSubscriberEmail(string deleteEmail)
		{
			EventfulDBManager.ExecuteInContext((db) =>
			{
				var Klaxons = getCollection(db);
				var query = getKlaxonsBySubscriberEmail(deleteEmail, Klaxons);
				foreach (var k in query)
				{
					k.SubscriberEmails.Remove(deleteEmail);
					Klaxons.Save(k);
				}
			});
		}
	}
}
