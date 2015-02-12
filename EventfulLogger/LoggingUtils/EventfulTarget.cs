using System;
using System.Collections.Generic;
using System.Dynamic;
using NLog;
using NLog.Targets;

namespace EventfulLogger.LoggingUtils
{
	[Target("Eventful")]
	public class EventfulTarget : TargetWithLayout
	{
		#region Members

		public string EventfulGroup { get; set; }
		public string ApplicationName { get; set; }
		public string ServiceUrl { get; set; }
		public string QueueUrl { get; set; }
		public string AWSAccessKey { get; set; }
		public string AWSSecretKey { get; set; }

		#endregion

		protected override void Write(LogEventInfo eventInfo)
		{
			dynamic eventInfoToLog;
			EventfulLogger.ELoggerBase logger;

			if (string.IsNullOrEmpty(ApplicationName))
				throw new ApplicationException("Error. EventfulTarget.ApplicationName must be defined in Nlog.config.");

			if (string.IsNullOrEmpty(ServiceUrl))
				throw new ApplicationException("Error. EventfulTarget.ServiceUrl must be defined in NLog.config.");

			if (string.IsNullOrEmpty(QueueUrl))
				throw new ApplicationException("Error. EventfulTarget.QueueUrl must be defined in NLog.config.");

			if (string.IsNullOrEmpty(AWSAccessKey))
				throw new ApplicationException("Error. EventfulTarget.AWSAccessKey must be defined in NLog.config.");

			if (string.IsNullOrEmpty(AWSSecretKey))
				throw new ApplicationException("Error. EventfulTarget.AWSSecretKey must be defined in NLog.config.");

			logger = EventfulLogger.ELoggerBase.Create(ServiceUrl, QueueUrl, AWSAccessKey, AWSSecretKey, determineEventfulGroup());

			eventInfoToLog = buildEventInfo(eventInfo);

			logger.LogEventAsync(eventInfo.Exception, eventInfo.FormattedMessage, eventInfoToLog);
		}

		private string determineEventfulGroup()
		{
			if (string.IsNullOrWhiteSpace(EventfulGroup))
				return "Unspecified";

			return EventfulGroup;
		}

		private dynamic buildEventInfo(LogEventInfo nLogEventInfo)
		{
			dynamic eventfulEventInfo = new ExpandoObject();
			var dict = (IDictionary<string, object>)eventfulEventInfo;

			eventfulEventInfo.ApplicationName = this.ApplicationName;
			eventfulEventInfo.LogLevel = nLogEventInfo.Level.ToString();
			eventfulEventInfo.LoggerName = nLogEventInfo.LoggerName;
			eventfulEventInfo.Timestamp = nLogEventInfo.TimeStamp;

			foreach (var key in nLogEventInfo.Properties.Keys)
			{
				var keyStr = key.ToString();
				if (!keyStr.StartsWith("Formatted"))
				{
					if (dict.ContainsKey(keyStr))
					{
						dict[keyStr] = nLogEventInfo.Properties[key];
					}
					else
					{
						dict.Add(keyStr, nLogEventInfo.Properties[key]);
					}
				}
			}

			return eventfulEventInfo;
		}
	}
}
