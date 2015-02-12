using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using EventfulBackend.EventfulAggregation;
using EventfulBackend.EventfulReporting;
using MongoDB.Bson;

namespace Eventful.Models
{
	public class EventfulReportModel
	{
		public string Id { get; set; }
		public IEnumerable<EventfulAggregateModel> AggregateRequests { get; set; }
		public string Title { get; set; }

		public EventfulReportModel() 
		{
		}

		public static EventfulReportModel FromEventfulReport(EventfulReport srcReport)
		{
			if (srcReport == null)
			{
				return null;
			}

			return Mapper.Map<EventfulReportModel>(srcReport);
		}

		public EventfulReport AsEventulReport()
		{
			return Mapper.Map<EventfulReport>(this);
		}

		public void Insert()
		{
			var er = this.AsEventulReport();
			er.Insert();
			this.Id = er.Id;
		}


		public void Update()
		{
			var er = this.AsEventulReport();
			er.Update();
		}

		public static IEnumerable<EventfulReportModel> GetAllReports()
		{
			var reports = EventfulReport.GetAllReports();
			return Mapper.Map<IEnumerable<EventfulReportModel>>(reports);
		}

		public static EventfulReportModel GetById(string id)
		{
			EventfulReport er = EventfulReport.GetById(id);
			return EventfulReportModel.FromEventfulReport(er);
		}
	}
}