using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Eventful.Shared.ExtensionMethods;

namespace EventfulLogger
{
	public class EloggerUtils
	{
		public static ExpandoObject BuildDynamic(string eventfulGroup, Exception exception, string message, object eventInfo, DateTime? deleteAfter = null)
		{
			dynamic dyn = eventInfo as ExpandoObject;

			if (dyn == null)
			{
				if (eventInfo == null)
				{
					dyn = new ExpandoObject();
				}
				else
				{
					dyn = eventInfo.ToDynamic();
				}
			}

			dyn.Timestamp = dynamicPropertyExists(dyn, "Timestamp") ? dyn.Timestamp : DateTime.Now;
			dyn.eventfulGroup = eventfulGroup;
			dyn.MachineName = dynamicPropertyExists(dyn, "MachineName") ? dyn.MachineName : EnvironmentVariables.MachineName;
			dyn.WindowsIdentity = dynamicPropertyExists(dyn, "WindowsIdentity") ? dyn.WindowsIdentity : EnvironmentVariables.ApplicationRunningAs;
			dyn.ApplicationName = dynamicPropertyExists(dyn, "ApplicationName") ? dyn.ApplicationName : EnvironmentVariables.ApplicationName;

			if (deleteAfter == null || deleteAfter.HasValue == false)
				dyn.DeleteAfter = dyn.Timestamp.AddDays(10);
			else
				dyn.DeleteAfter = deleteAfter;

			if (message != null)
				dyn.Message = message;

			addExceptionToDynamic(dyn, exception);
			addHttpContextToDynamic(dyn, HttpContext.Current);

			return dyn;
		}

		private static bool dynamicPropertyExists(dynamic dyn, string propertyName)
		{
			IDictionary<string, object> dict = dyn as IDictionary<string, object>;
			return dict != null && dict.ContainsKey(propertyName);
		}

		private static void addHttpContextToDynamic(dynamic dyn, HttpContext currentContext)
		{
			HttpRequest request;

			if (currentContext != null && currentContext.Request != null)
			{
				request = currentContext.Request;

				dyn.HttpPage = dynamicPropertyExists(dyn, "HttpPage") ? dyn.HttpPage : determinePage(request);
				dyn.HttpRawUrl = dynamicPropertyExists(dyn, "HttpRawUrl") ? dyn.HttpRawUrl : determineRawUrl(request);
				dyn.HttpUrlReferrer = dynamicPropertyExists(dyn, "HttpUrlReferrer") ? dyn.HttpUrlReferrer : determineUrlReferrer(request);
				dyn.HttpQueryString = dynamicPropertyExists(dyn, "HttpQueryString") ? dyn.HttpQueryString.ToString() : request.QueryString.ToString();
				dyn.HttpQueryString = StringExtensions.SimpleTruncate((string)dyn.HttpQueryString, 500);
				dyn.HttpUser = dynamicPropertyExists(dyn, "HttpUser") ? dyn.HttpUser : determineUser(currentContext);
				dyn.HttpRequestingIP = dynamicPropertyExists(dyn, "HttpRequestingIP") ? dyn.HttpRequestingIP : request.UserHostAddress.SimpleTruncate(50);
				dyn.HttpUserAgent = dynamicPropertyExists(dyn, "HttpUserAgent") ? dyn.HttpUserAgent : request.UserAgent.SimpleTruncate(500);
			}
		}

		private static string determinePage(HttpRequest request)
		{
			string page = "null";

			if (request.Url != null)
				page = request.Url.AbsoluteUri.SimpleTruncate(500);

			return page;
		}

		private static string determineRawUrl(HttpRequest request)
		{
			string rawUrl = "null";

			if (request.RawUrl != null)
				rawUrl = request.RawUrl.SimpleTruncate(500);

			return rawUrl;
		}

		private static string determineUrlReferrer(HttpRequest request)
		{
			string urlReferrer = "Unavailable";

			if (!String.IsNullOrEmpty(request.ServerVariables["http_referer"]))
				urlReferrer = request.ServerVariables["http_referer"].SimpleTruncate(500);

			return urlReferrer;
		}

		private static string determineUser(HttpContext currentContext)
		{
			string user = "Anon";

			if (currentContext.User != null
				&& currentContext.User.Identity.IsAuthenticated
				&& string.IsNullOrEmpty(currentContext.User.Identity.Name) == false)
			{
				user = currentContext.User.Identity.Name;
			}

			return user.SimpleTruncate(150);
		}

		private static void addExceptionToDynamic(dynamic dyn, Exception exception)
		{
			if (exception != null)
			{
				dyn.ExceptionStackTrace = exception.ToString().Trim();

				if (exception is HttpUnhandledException && exception.InnerException != null)
					dyn.ExceptionType = string.Format("{0} ({1})", exception.InnerException.GetType().ToString(), exception.GetType().ToString());
				else
					dyn.ExceptionType = exception.GetType().ToString().SimpleTruncate(300);

				if (exception.Message != null)
					dyn.ExceptionMessage = exception.Message.SimpleTruncate(500);
				else
					dyn.ExceptionMessage = null;
			}
		}
	}
}
