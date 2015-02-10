using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using NLog;
using EventfulLogger.LoggingUtils;

namespace eventful
{
    public class MvcApplication : System.Web.HttpApplication
    {
		private static Logger logger = LogManager.GetCurrentClassLogger();

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

		void Application_Error(object sender, EventArgs e)
		{
			Exception ex = Server.GetLastError();
			try
			{
				logger.WyzAntError(ex, "Unhandled application error.");
			}
			catch (Exception logExc)
			{

				logger.WyzAntError("Fatal error encountered trying to log application error.  Logging error:\r\n{0}\r\n\r\nOriginal error:\r\n{1}\r\n", logExc, ex);
			}
		}
    }
}
