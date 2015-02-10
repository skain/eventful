using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLog;
using EventfulLogger.LoggingUtils;

namespace eventful.Controllers
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
			logger.WyzAntDebug("This is a debug message.");
			logger.WyzAntFatal("This is a fatal message.");
			logger.WyzAntInfo("This is an info message.");
			logger.WyzAntTrace("This is a trace message.");
			logger.WyzAntWarn("This is a warn message.");

			try
			{
				int y = 0;
				int x = 1 / y;
			}
			catch (Exception e)
			{
				logger.WyzAntError(e, "This is an error message (divide by 0).");
			}

			return View();
		}
    }
}
