using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Web;
using EventfulLogger;
using NLog;
using Eventful.Shared.ExtensionMethods;

namespace EventfulLogger.LoggingUtils
{
	public static class LoggerExtender
	{
		private static Logger _localLogger = LogManager.GetCurrentClassLogger();

		#region Trace
		/// <summary>
		/// Log a Trace level log event with HttpContext data if available.
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="message"></param>
		/// <param name="msgParms"></param>
		public static void EventfulTrace(this Logger logger, string message, params object[] msgParms)
		{
			EventfulTrace(logger, null, null, message, msgParms: msgParms);
		}

		/// <summary>
		/// Log a Trace level log event with formatted exception data and HttpContext data if available.
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="exception"></param>
		/// <param name="message"></param>
		/// <param name="msgParms"></param>
		public static void EventfulTrace(this Logger logger, Exception exception, string message, params object[] msgParms)
		{
			EventfulTrace(logger, null, exception, message, msgParms: msgParms);
		}

		/// <summary>
		/// Log a Trace level log event with an eventful payload, formatted exception data and HttpContext data if available.
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="message"></param>
		/// <param name="msgParms"></param>
		public static void EventfulTrace(this Logger logger, object eventfulPayload, Exception exception, string message, params object[] msgParms)
		{
			EventfulLog(logger, LogLevel.Trace, eventfulPayload, exception, message, msgParms);
		}

		#endregion

		#region Debug
		/// <summary>
		/// Log a Debug level log event with HttpContext data if available.
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="message"></param>
		/// <param name="msgParms"></param>
		public static void EventfulDebug(this Logger logger, string message, params object[] msgParms)
		{
			EventfulDebug(logger, null, null, message, msgParms: msgParms);
		}

		/// <summary>
		/// Log a Debug level log event with formatted exception data and HttpContext data if available.
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="exception"></param>
		/// <param name="message"></param>
		/// <param name="msgParms"></param>
		public static void EventfulDebug(this Logger logger, Exception exception, string message, params object[] msgParms)
		{
			EventfulDebug(logger, null, exception, message, msgParms: msgParms);
		}

		/// <summary>
		/// Log a Debug level log event with an eventful payload, formatted exception data and HttpContext data if available.
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="message"></param>
		/// <param name="msgParms"></param>
		public static void EventfulDebug(this Logger logger, object eventfulPayload, Exception exception, string message, params object[] msgParms)
		{
			EventfulLog(logger, LogLevel.Debug, eventfulPayload, exception, message, msgParms);
		}
		#endregion

		#region Info
		/// <summary>
		/// Log an Info level log event with HttpContext data if available.
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="message"></param>
		/// <param name="msgParms"></param>
		public static void EventfulInfo(this Logger logger, string message, params object[] msgParms)
		{
			EventfulInfo(logger, null, null, message, msgParms: msgParms);
		}

		public static void EventfulInfo(this Logger logger, Exception exception, string message, params object[] msgParms)
		{
			EventfulInfo(logger, null, exception, message, msgParms: msgParms);
		}
		/// <summary>
		/// Log an Info level log event with an eventful payload, formatted exception data and HttpContext data if available.
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="message"></param>
		/// <param name="msgParms"></param>
		public static void EventfulInfo(this Logger logger, object eventfulPayload, Exception exception, string message, params object[] msgParms)
		{
			EventfulLog(logger, LogLevel.Info, eventfulPayload, exception, message, msgParms);
		}
		#endregion Info

		#region Warn
		/// <summary>
		/// Log a Warn level log event with HttpContext data if available.
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="message"></param>
		/// <param name="msgParms"></param>
		public static void EventfulWarn(this Logger logger, string message, params object[] msgParms)
		{
			EventfulWarn(logger, null, null, message, msgParms: msgParms);
		}

		/// <summary>
		/// Log a Warn level log event with formatted exception data and HttpContext data if available.
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="exception"></param>
		/// <param name="message"></param>
		/// <param name="msgParms"></param>
		public static void EventfulWarn(this Logger logger, Exception exception, string message, params object[] msgParms)
		{
			EventfulWarn(logger, null, exception, message, msgParms: msgParms);
		}

		/// <summary>
		/// Log a Warn level log event with an eventful payload, formatted exception data and HttpContext data if available.
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="message"></param>
		/// <param name="msgParms"></param>
		public static void EventfulWarn(this Logger logger, object eventfulPayload, Exception exception, string message, params object[] msgParms)
		{
			EventfulLog(logger, LogLevel.Warn, eventfulPayload, exception, message, msgParms);
		}
		#endregion

		#region Error
		/// <summary>
		/// Log an Error level log event with HttpContext data if available.
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="message"></param>
		/// <param name="msgParms"></param>
		public static void EventfulError(this Logger logger, string message, params object[] msgParms)
		{
			EventfulError(logger, eventfulPayload: null, exception: null, message: message, msgParms: msgParms);
		}

		public static void EventfulError(this Logger logger, Exception exception, string message, params object[] msgParms)
		{
			EventfulError(logger, null, exception, message, msgParms: msgParms);
		}
		/// <summary>
		/// Log an Error level log event with formatted exception data and HttpContext data if available.
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="message"></param>
		/// <param name="msgParms"></param>
		public static void EventfulError(this Logger logger, object eventfulPayload, Exception exception, string message, params object[] msgParms)
		{
			EventfulLog(logger, LogLevel.Error, eventfulPayload, exception, message, msgParms);
		}
		#endregion

		#region Fatal
		/// <summary>
		/// Log a Fatal level log event with HttpContext data if available.
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="message"></param>
		/// <param name="msgParms"></param>
		public static void EventfulFatal(this Logger logger, string message, params object[] msgParms)
		{
			EventfulFatal(logger, null, null, message, msgParms: msgParms);
		}

		/// <summary>
		/// Log a Fatal level log event with formatted exception data and HttpContext data if available.
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="message"></param>
		/// <param name="msgParms"></param>
		public static void EventfulFatal(this Logger logger, Exception exception, string message, params object[] msgParms)
		{
			EventfulLog(logger, LogLevel.Fatal, null, exception, message, msgParms);
		}

		/// <summary>
		/// Log a Fatal level log event with formatted exception data and HttpContext data if available.
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="message"></param>
		/// <param name="msgParms"></param>
		public static void EventfulFatal(this Logger logger, object eventfulPayload, Exception exception, string message, params object[] msgParms)
		{
			EventfulLog(logger, LogLevel.Fatal, eventfulPayload, exception, message, msgParms);
		}
		#endregion

		#region Log
		/// <summary>
		/// Log a log event with the specified LogLevel with formatted exception data and HttpContext data if available.
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="logLevel"></param>
		/// <param name="exception"></param>
		/// <param name="message"></param>
		/// <param name="msgParms"></param>
		public static void EventfulLog(this Logger logger, LogLevel logLevel, object eventfulPayload, Exception exception, string message, params object[] msgParms)
		{
			EventfulLog(logger, null, logLevel, eventfulPayload, exception, message, msgParms);
		}

		public static void EventfulLog(this Logger logger, LogEventInfo lei, LogLevel logLevel, object eventfulPayload, Exception exception, string message, params object[] msgParms)
		{
			EventfulLog(logger, lei, logLevel, eventfulPayload, exception, null, message, null, null, null, msgParms);
		}

		/// <summary>
		/// Log a log event with the specified LogLevel with formatted exception data and HttpContext data. Use this overload when the HttpContext is not available via HttpContext.Current
		/// (ie. in ashx files.) If the parameters ending in '_override' are supplied then the values supplied will be used instead of the default values derived from the current context. 
		/// If they are null then the default contextual values will be used.
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="logLevel"></param>
		/// <param name="exception"></param>
		/// <param name="message"></param>
		/// <param name="msgParms"></param>
		public static void EventfulLog(this Logger logger, LogEventInfo lei, LogLevel logLevel, object eventfulPayload, Exception exception, HttpContext httpContext, string message, string httpUser_override, string machineName_override, string applicationName_override, params object[] msgParms)
		{
			try
			{
				string formattedMsg = null;
				if (!string.IsNullOrWhiteSpace(message))
				{
					if (msgParms == null || msgParms.Length == 0)
					{
						formattedMsg = message;
					}
					else
					{
						formattedMsg = string.Format(message, msgParms);
					}
				}
				lei = lei ?? new LogEventInfo(logLevel, logger.Name, formattedMsg);
				addExceptionToLoggerContext(exception, lei);
				addHttpContextToLoggerContext(lei, httpContext ?? HttpContext.Current, httpUser_override);
				addEnvironmentToLoggerContext(lei, machineName_override, applicationName_override);
				addeventfulPayloadToLoggerContext(lei, eventfulPayload);

				logger.Log(lei);
			}
			catch (Exception e)
			{
				var strings = msgParms.Select(mp => mp == null ? "NULL" : mp.ToString());
				var formatted = string.Join("\r\n", strings);
				_localLogger.Error("Error in Eventful LoggerExtender.\r\n\r\nOriginal LogMessage:\r\n{0}\r\nLogMessage Arguments:\r\n{1}\r\nException:{2}", message, formatted, e.ToString());
			}
		}
		#endregion

		#region Testing
		public static void TestEventfulLogging(this Logger logger)
		{
			logger.EventfulDebug("This is a debug message: {0}", DateTime.Now.ToString());
			Thread.Sleep(1000);
			logger.EventfulTrace("This is a trace message: {0}", DateTime.Now.ToString());
			Thread.Sleep(1000);
			logger.EventfulInfo("This is an info message: {0}", DateTime.Now.ToString());
			Thread.Sleep(1000);
			logger.EventfulWarn("This is a warn message: {0}", DateTime.Now.ToString());
			Thread.Sleep(1000);
			logger.EventfulFatal("This is a fatal message: {0}", DateTime.Now.ToString());
			Thread.Sleep(1000);

			try
			{
				throw new ApplicationException("This is the exception message");
			}
			catch (Exception ex)
			{
				logger.EventfulError(ex, "This is an error message: {0}", DateTime.Now.ToString());
			}

			Thread.Sleep(3000);
		}

		public static void TestEventfulLoggingWitheventfulPayloads(this Logger logger)
		{
			var payload = new { StringVal = "one", IntVal = 42 };
			logger.EventfulDebug(payload, null, "This is a debug message (with payload): {0}", DateTime.Now.ToString());
			Thread.Sleep(1000);
			logger.EventfulTrace(payload, null, "This is a trace message (with payload): {0}", DateTime.Now.ToString());
			Thread.Sleep(1000);
			logger.EventfulInfo(payload, null, "This is an info message (with payload): {0}", DateTime.Now.ToString());
			Thread.Sleep(1000);
			logger.EventfulWarn(payload, null, "This is a warn message (with payload): {0}", DateTime.Now.ToString());
			Thread.Sleep(1000);
			logger.EventfulFatal(payload, null, "This is a fatal message (with payload): {0}", DateTime.Now.ToString());
			Thread.Sleep(1000);

			try
			{
				throw new ApplicationException("This is the exception message");
			}
			catch (Exception ex)
			{
				logger.EventfulError(payload, ex, "This is an error  (with payload): {0}", DateTime.Now.ToString());
			}

			Thread.Sleep(3000);
		}
		#endregion

		#region Helpers
		private static void addEnvironmentToLoggerContext(LogEventInfo lei, string machineName_override, string applicationName_override)
		{
			lei.Properties["MachineName"] = string.IsNullOrWhiteSpace(machineName_override) ? EnvironmentVariables.MachineName : machineName_override;
			if (!string.IsNullOrWhiteSpace(applicationName_override))
			{
				lei.Properties["ApplicationName"] = applicationName_override;
			}
		}

		private static void addeventfulPayloadToLoggerContext(LogEventInfo lei, object eventfulPayload)
		{
			if (eventfulPayload == null)
			{
				return;
			}

			dynamic dyn = eventfulPayload as ExpandoObject;

			if (dyn == null)
			{
				dyn = ConvertToDynamic(eventfulPayload);
			}

			var dict = (IDictionary<string, object>)dyn;
			foreach (var key in dict.Keys)
			{
				lei.Properties[key] = dict[key];
			}
		}

		public static ExpandoObject ConvertToDynamic(object value)
		{
			IDictionary<string, object> expando = new ExpandoObject();

			Type type = value.GetType();
			var properties = TypeDescriptor.GetProperties(type);
			foreach (PropertyDescriptor property in properties)
			{
				object val = property.GetValue(value);
				expando.Add(property.Name, val);
			}

			return expando as ExpandoObject;
		}

		private static void addHttpContextToLoggerContext(LogEventInfo lei, HttpContext currentContext, string httpUser)
		{
			if (currentContext != null && currentContext.Request != null)
			{
				//If the querystring contains a dangerous value, ToStringing it will cause an exception.
				//That would create an infinite loop here.
				string queryString = string.Empty;
				try
				{
					queryString = currentContext.Request.QueryString.ToString();
				}
				catch { }
				string page = (currentContext.Request.Url != null ? currentContext.Request.Url.AbsoluteUri : "null");
				string rawUrl = (currentContext.Request.RawUrl != null ? currentContext.Request.RawUrl : "null");
				string urlReferrer = "Unavailable";
				string requestingIP = currentContext.Request.UserHostAddress;
				string userAgent = currentContext.Request.UserAgent;
				string user = httpUser;

				if (string.IsNullOrWhiteSpace(user))
				{
					user = "Anon";
					try
					{
						if (currentContext.User != null && currentContext.User.Identity.IsAuthenticated && string.IsNullOrEmpty(currentContext.User.Identity.Name) == false)
						{
							user = currentContext.User.Identity.Name;
						}
					}
					catch
					{
						user = "Anon";
					}
				}

				if (!String.IsNullOrEmpty(HttpContext.Current.Request.ServerVariables["http_referer"]))
				{
					urlReferrer = HttpContext.Current.Request.ServerVariables["http_referer"];
				}
				//HttpContext.Current.Request.UrlReferrer != null is throwing error when a bad url is supplied
				//if (HttpContext.Current.Request.UrlReferrer != null)
				//{
				//    urlReferrer = HttpContext.Current.Request.UrlReferrer.AbsoluteUri;
				//}

				lei.Properties["HttpUser"] = user.SimpleTruncate(150);
				lei.Properties["HttpUrlReferrer"] = urlReferrer.SimpleTruncate(500);
				lei.Properties["HttpQueryString"] = queryString.SimpleTruncate(500);
				lei.Properties["HttpRequestingIP"] = requestingIP.SimpleTruncate(50);
				lei.Properties["HttpUserAgent"] = userAgent.SimpleTruncate(500);
				lei.Properties["HttpPage"] = page.SimpleTruncate(500);
				lei.Properties["HttpRawUrl"] = rawUrl.SimpleTruncate(500);
				lei.Properties["FormattedHttpData"] = string.Format("\r\nUser: {0}\r\nUrlReferrer: {1}\r\nQueryString: {2}\r\nRequestingIP: {3}\r\nUserAgent: {4}\r\nPage: {5}\r\nRawUrl: {6}\r\n", user, urlReferrer, queryString, requestingIP, userAgent, page, rawUrl);
			}
		}

		private static void addExceptionToLoggerContext(Exception exception, LogEventInfo lei)
		{
			if (exception != null)
			{
				if (exception is System.Web.HttpUnhandledException && exception.InnerException != null)
				{
					string innerType = exception.InnerException.GetType().ToString().SimpleTruncate(300);
					string outerType = exception.GetType().ToString();
					lei.Properties["ExceptionType"] = innerType;
					lei.Properties["IsUnhandledWebException"] = 1;
				}
				else
				{
					lei.Properties["ExceptionType"] = exception.GetType().ToString().SimpleTruncate(300);
				}
				lei.Properties["ExceptionStackTrace"] = exception.ToString().Trim();
				lei.Properties["ExceptionMessage"] = exception.Message == null ? null : exception.Message.SimpleTruncate(500);
				lei.Properties["FormattedExceptionData"] = string.Format("\r\nException Data:\r\n{0}", exception.ToString());
			}
		}
		#endregion
	}
}

//info, error