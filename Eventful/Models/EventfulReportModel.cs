using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using eventfulBackend.eventfulAggregation;
using eventfulBackend.eventfulReporting;
using MongoDB.Bson;

namespace eventful.Models
{
	public class eventfulReportModel
	{
		public string Id { get; set; }
		public IEnumerable<eventfulAggregateModel> AggregateRequests { get; set; }
		public string Title { get; set; }

		public eventfulReportModel() 
		{
		}

		public static eventfulReportModel FromeventfulReport(eventfulReport srcReport)
		{
			if (srcReport == null)
			{
				return null;
			}

			return Mapper.Map<eventfulReportModel>(srcReport);
		}

		public eventfulReport AsEventulReport()
		{
			return Mapper.Map<eventfulReport>(this);
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

		public static IEnumerable<eventfulReportModel> GetAllReports()
		{
			var reports = eventfulReport.GetAllReports();
			return Mapper.Map<IEnumerable<eventfulReportModel>>(reports);
		}

		public static eventfulReportModel GetById(string id)
		{
			eventfulReport er = eventfulReport.GetById(id);
			return eventfulReportModel.FromeventfulReport(er);
		}
	}
}