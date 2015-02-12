using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLog;
using EventfulLogger.LoggingUtils;

namespace Eventful.Controllers
{
	[Authorize]
	public class HomeController : Controller
    {
		private static Logger logger = LogManager.GetCurrentClassLogger();
        //
        // GET: /eventful/Home/

        public ActionResult Index()
        {
            return View();
        }

		public ActionResult LogTest()
		{
			logger.EventfulDebug("This is a debug message.");
			logger.EventfulFatal("This is a fatal message.");
			logger.EventfulInfo("This is an info message.");
			logger.EventfulTrace("This is a trace message.");
			logger.EventfulWarn("This is a warn message.");

			try
			{
				int y = 0;
				int x = 1 / y;
			}
			catch (Exception e)
			{
				logger.EventfulError(e, "This is an error message (divide by 0).");
			}

			return View();
		}
    }
}
