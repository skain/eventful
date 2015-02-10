using System;

namespace EventfulLogger
{
	/// <summary>
	/// The ELogger class is the main class to use when writing Events to eventful. It encapsulates the logic for storing settings in the application configuration file and then passes requests
	/// log through to the ELoggerBase class for processing.
	/// 
	/// In order to use this class you must define the settings AWSServiceUrl, AWSQueueUrl, AWSAccessKey and AWSSecretKey in the EventfulLogger section of your application configuration file.
	/// </summary>
	public class ELogger
	{
		#region Constructor

		public ELogger(string eventfulGroup)
		{
			string suffix = Properties.Settings.Default.eventfulGroupSuffix;

			if (!string.IsNullOrWhiteSpace(suffix))
				eventfulGroup = string.Concat(eventfulGroup, suffix);

			_logger = ELoggerBase.Create(Properties.Settings.Default.AWSServiceUrl, 
				                    Properties.Settings.Default.AWSQueueUrl, 
									Properties.Settings.Default.AWSAccessKey, 
				                    Properties.Settings.Default.AWSSecretKey,
									eventfulGroup);
		}

		#endregion

		#region Members

		private ELoggerBase _logger;

		#endregion

		#region Properties

		public string eventfulGroup 
		{
			get
			{
				return _logger.EventfulGroup;
			}
		}

		#endregion

		#region LogEvent

		public void LogEvent(object eventInfo, DateTime? deleteAfter = null)
		{
			LogEvent(null, null, eventInfo, deleteAfter);
		}

		public void LogEvent(string message, object eventInfo, DateTime? deleteAfter = null)
		{
			LogEvent(null, message, eventInfo, deleteAfter);
		}

		public void LogEvent(Exception exception, string message, object eventInfo, DateTime? deleteAfter = null)
		{
			_logger.LogEvent(exception, message, eventInfo, deleteAfter);
		}

		public void LogEventAsync(object eventInfo, DateTime? deleteAfter = null)
		{
			LogEventAsync(null, null, eventInfo, deleteAfter);
		}

		public void LogEventAsync(string message, object eventInfo, DateTime? deleteAfter = null)
		{
			LogEventAsync(null, message, eventInfo, deleteAfter);
		}

		public void LogEventAsync(Exception exception, string message, object eventInfo, DateTime? deleteAfter = null)
		{
			_logger.LogEventAsync(exception, message, eventInfo, deleteAfter);
		}


		#endregion
	}
}