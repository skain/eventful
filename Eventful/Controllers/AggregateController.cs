using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Eventful.Models;
using EventfulBackend.Utils;
using NLog;
using EventfulLogger.LoggingUtils;

namespace Eventful.Controllers
{
	//[Authorize]
    public class AggregateController : Controller
    {
		private static Logger logger = LogManager.GetCurrentClassLogger();
        //
        // GET: /eventful/Aggregate/

        public ActionResult Index()
        {
			return View();
        }

		[HttpPost]
		public ActionResult Index(EventfulAggregateModel model)
		{
			try
			{ 
				model.ExecuteAggregate();
				return Json(model);
			}
			catch (Exception e)
			{
				logger.EventfulError(e, "Error processing aggregate request.");
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
			var result = EventfulBackend.EventfulAggregation.TimeSeriesAggregator.RunAggregation(startTime, endTime, TimeZoneUtils.CST);
			return Json(result);
		}
    }
}
