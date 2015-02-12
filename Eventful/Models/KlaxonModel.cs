using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using EventfulBackend;
using EventfulBackend.Klaxon;
using EventfulBackend.Utils;
using EventfulLogger;
using NLog;
using EventfulLogger.LoggingUtils;

namespace Eventful.Models
{
	public class KlaxonModel
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		public string Id { get; set; }
		public string Name { get; set; }
		public string EqlQuery { get; set; }
		public string ComparisonOperator { get; set; }
		public int ComparisonThreshold { get; set; }
		public List<string> SubscriberEmails { get; private set; }
		public string CheckEvery { get; set; }
		public DateTime LastCheckedAt { get; set; }
		public DateTime NextCheckAt { get; set; }
		public long? NumMatched { get; private set; }
		public string FieldToAggregate { get; set; }
		public string AggregateOperation { get; set; }
		public string StartTime { get; set; }
		public string EndTime { get; set; }
		public bool TestResult { get; private set; }
		public EventfulReportModel ReportToLink { get; set; }

		public bool CurrentUserIsSubscribed
		{
			get
			{
				if (SubscriberEmails == null)
				{
					return false;
				}
				EventfulUserModel user = EventfulUserModel.GetOrCreateCurrentUser();
				return SubscriberEmails.Any(s => user.Emails.Contains(s));
			}
		}
		public KlaxonModel()
		{
		}

		public static KlaxonModel FromKlaxon(Klaxon Klaxon)
		{
			return loadFromKlaxon(Klaxon);
		}

		//public DateTime ToCST(DateTime dt)
		//{
		//	return TimeZoneInfo.ConvertTime(dt, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
		//}

		private static KlaxonModel loadFromKlaxon(Klaxon Klaxon)
		{
			KlaxonModel km = Mapper.Map<KlaxonModel>(Klaxon);
			km.ConvertAllTimeMembers(TimeZoneInfo.Local, TimeZoneUtils.CST);
			km.TestResult = Klaxon.TestResult;
			var r = Klaxon.GetReport();
			km.ReportToLink = EventfulReportModel.FromEventfulReport(r);
			return km;
		}

		public Klaxon AsKlaxon()
		{
			Klaxon k = Mapper.Map<Klaxon>(this);
			k.ReportId = this.ReportToLink == null ? null : this.ReportToLink.Id;
			return k;
		}

		public void Create()
		{
			var user = EventfulUserModel.GetOrCreateCurrentUser();
			SubscriberEmails = new List<string>();
			this.SubscriberEmails.Add(user.Emails.First());
			var k = this.AsKlaxon();
			k.Insert();
			this.Id = k.Id;
		}

		

		public static void DeleteById(string id)
		{
			Klaxon.DeleteById(id);
		}

		/// <summary>
		/// The updateIsStateless parameter specifies that values that aren't updated by a typical edit operation will be preserved even though they may not be included on the model to be updated.
		/// </summary>
		/// <param name="updateIsStateless"></param>
		public void Update(bool updateIsStateless)
		{
			if (updateIsStateless)
			{
				KlaxonModel oldKM = KlaxonModel.GetById(this.Id);
				this.SubscriberEmails = new List<string>(oldKM.SubscriberEmails);
				this.NumMatched = oldKM.NumMatched;
			}
			this.ConvertAllTimeMembers(TimeZoneUtils.CST, TimeZoneInfo.Local);
			var k = this.AsKlaxon();

			k.Update();
		}

		public static IEnumerable<KlaxonModel> GetAll()
		{
			IEnumerable<Klaxon> ks = Klaxon.GetAll();
			
			var kms = Mapper.Map<IEnumerable<KlaxonModel>>(ks);
			kms.Each(k =>
			{
				k.ConvertAllTimeMembers(TimeZoneInfo.Local, TimeZoneUtils.CST);
			});

			return kms;
		}

		public void ConvertAllTimeMembers(TimeZoneInfo srcTimeZone, TimeZoneInfo destTimeZone)
		{
			NextCheckAt = TimeZoneUtils.ConvertTimeIfNeeded(NextCheckAt, srcTimeZone, destTimeZone);
			LastCheckedAt = TimeZoneUtils.ConvertTimeIfNeeded(LastCheckedAt, srcTimeZone, destTimeZone);
		}

		public void RunTest(bool updateCheckTimes)
		{
			var k = this.AsKlaxon();
			k.RunTest(updateCheckTimes);
			loadFromKlaxon(k);
		}

		internal static KlaxonModel CreateNewModel()
		{
			Klaxon k = new Klaxon();
			return Mapper.Map<KlaxonModel>(k);
		}

		internal static KlaxonModel GetById(string id)
		{
			return loadFromKlaxon(Klaxon.GetById(id));
		}

		internal bool ToggleSubscriptionForCurrentUser()
		{
			if (CurrentUserIsSubscribed)
			{
				EventfulUserModel user = EventfulUserModel.GetCurrentUser();
				SubscriberEmails.RemoveAll(se => user.Emails.Contains(se));
				this.AsKlaxon().Update();
				return false;
			}
			else
			{
				EventfulUserModel eum = EventfulUserModel.GetOrCreateCurrentUser();
				SubscriberEmails.Add(eum.Emails.First());
				this.AsKlaxon().Update();
				return true;
			}
		}

		internal static void UpdateSubscriberEmail(string oldEmail, string newEmail)
		{
			Klaxon.UpdateSubscriberEmail(oldEmail, newEmail);
		}

		internal static void DeleteSubscriberEmail(string deleteEmail)
		{
			Klaxon.DeleteSubscriberEmail(deleteEmail);
		}

	}
}