using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using eventfulBackend.eventfulReporting;
using eventful.Models;

namespace eventful.Controllers
{
	[Authorize]
	public class ReportsController : Controller
    {
        //
        // GET: /eventful/Reports/

        public ActionResult Index()
        {
			ReportListModel model = ReportListModel.GetReports();
            return View(model);
        }


		public new ActionResult View(string id)
		{
			var model = eventfulReportModel.GetById(id);
			return View(model);
		}


		public ActionResult Create()
		{
			return View(new eventfulReportModel());
		}

		[HttpPost]
		public JsonResult Create(eventfulReportModel model)
		{
			model.Insert();
			return Json(model);
		}



		public ActionResult Edit(string id)
		{
			var model = eventfulReportModel.GetById(id);
			return View(model);
		}

		[HttpPost]
		public JsonResult Edit(eventfulReportModel model)
		{
			model.Update();
			return Json(model);
		}

		
		public ActionResult Delete(string id)
		{
			eventfulReport.DeleteById(id);
			return RedirectToAction("Index");
		}


		[HttpPost]
		public ActionResult GetAll()
		{
			ReportListModel model = ReportListModel.GetReports();
			return Json(model);
		}


    }
}
