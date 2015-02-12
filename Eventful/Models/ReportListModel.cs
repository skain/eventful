using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EventfulBackend.EventfulReporting;

namespace Eventful.Models
{
	public class ReportListModel
	{
		public IEnumerable<EventfulReportModel> EventfulReports { get; private set;}

		public static ReportListModel GetReports()
		{
			ReportListModel retVal = new ReportListModel();
			retVal.EventfulReports = EventfulReportModel.GetAllReports();
			return retVal;
		}
	}
}