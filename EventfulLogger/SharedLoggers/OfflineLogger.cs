using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace EventfulLogger.SharedLoggers
{
	public static class OfflineLogger
	{
		private static Lazy<Logger> _nLogger = new Lazy<Logger>(() =>
		{
			return LogManager.GetCurrentClassLogger();
		});
		private static Lazy<ELogger> _eLogger = new Lazy<ELogger>(() =>
		{
			return new ELogger("OfflineLogger");
		});

		/// <summary>
		/// Logs an application start message to both eventful and Nlog. eventful event can be found with combo of application name (including .exe and command-line args if supplied) and Message == 'Application started.'
		/// </summary>
		public static void LogApplicationStart(string[] args = null)
		{
			dynamic eventInfo = getEventInfoObject(args);
			_eLogger.Value.LogEvent("Application started.", eventInfo, DateTime.Now.AddDays(3));
			_nLogger.Value.Debug("Application started.");
		}

		/// <summary>
		/// Logs an application finished message to both eventful and Nlog. eventful event can be found with combo of application name (including .exe and command-line args if supplied) and Message == 'Application finished.'
		/// </summary>
		public static void LogApplicationEnd(string[] args = null)
		{
			dynamic eventInfo = getEventInfoObject(args);
			_eLogger.Value.LogEvent("Application finished.", eventInfo, DateTime.Now.AddDays(3));
			_nLogger.Value.Debug("Application finished.");
			Thread.Sleep(2000); //sleep for 2 seconds to give the loggers time to get the end application message out the door
		}

		private static dynamic getEventInfoObject(string[] args)
		{
			string argsStr = "NONE";
			if (args != null && args.Length > 0)
			{
				argsStr = string.Join(" ", args);
			}
			dynamic eventInfo = new { CommandLineArgs = argsStr };
			return eventInfo;
		}
	}
}
