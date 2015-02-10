using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eventfulBackend.eventfulReporting;

namespace eventful.Models
{
	public class ReportListModel
	{
		public IEnumerable<eventfulReportModel> eventfulReports { get; private set;}

		public static ReportListModel GetReports()
		{
			ReportListModel retVal = new ReportListModel();
			retVal.eventfulReports = eventfulReportModel.GetAllReports();
			return retVal;
		}
	}
}