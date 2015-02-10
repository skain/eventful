using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using eventful.Models;
using eventfulBackend.Utils;
using NLog;
using EventfulLogger.LoggingUtils;

namespace eventful.Controllers
{
	[Authorize]
    public class AggregateController : Controller
    {
		private static Logger logger = LogManager.GetCurrentClassLogger();
        //
        // GET: /eventful/Aggregate/

        public ActionResult Index()
        {
			return View(new eventfulAggregateModel { MaxResultsBeforeOutliers = 5 });
        }

		[HttpPost]
		public ActionResult Index(eventfulAggregateModel model)
		{
			try
			{ 
				model.ExecuteAggregate();
				return Json(model);
			}
			catch (Exception e)
			{
				logger.WyzAntError(e, "Error processing aggregate request.");
				return View("Error");
			}

		}

		public ActionResult TimeTest()
		{
			return View();
		}

		[HttpPost]
		public ActionResult TimeTest(string startTime, string endTime)
		{
			var result = eventfulBackend.eventfulAggregation.TimeSeriesAggregator.RunAggregation(startTime, endTime, TimeZoneUtils.CST);
			return Json(result);
		}
    }
}
