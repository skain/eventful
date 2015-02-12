using System;
using System.Dynamic;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Web;
using Amazon.SQS;
using Amazon.SQS.Model;
using Eventful.Shared.ExtensionMethods;
using Newtonsoft.Json;

namespace EventfulLogger
{
	/// <summary>
	/// The ELoggerBase class encapsulates the actual code that handles posting of Events to eventful.
	/// </summary>
	public class ELoggerBase
	{
		#region Constructor

		private ELoggerBase(string awsServiceUrl, string awsQueueUrl, string awsAccessKey, string awsSecretKey, string eventfulGroup)
		{
			_sqsClient = new AmazonSQSClient(awsAccessKey, awsSecretKey, new AmazonSQSConfig() { ServiceURL = awsServiceUrl });
			_awsQueueURL = awsQueueUrl;
			_awsAccessKey = awsAccessKey;
			EventfulGroup = eventfulGroup;
		}

		public static ELoggerBase Create(string awsServiceUrl, string awsQueueUrl, string awsAccessKey, string awsSecretKey, string eventfulGroup)
		{
			if (string.IsNullOrWhiteSpace(awsServiceUrl))
				throw new EventfulLoggerNotConfiguredException("AWS service URL is required.");

			if (string.IsNullOrWhiteSpace(awsQueueUrl))
				throw new EventfulLoggerNotConfiguredException("AWS queue URL is required.");

			if (string.IsNullOrWhiteSpace(awsAccessKey))
				throw new EventfulLoggerNotConfiguredException("AWS access key is required.");

			if (string.IsNullOrWhiteSpace(awsSecretKey))
				throw new EventfulLoggerNotConfiguredException("AWS secret key is required.");

			return new ELoggerBase(awsServiceUrl, awsQueueUrl, awsAccessKey, awsSecretKey, eventfulGroup);
		}

		#endregion

		#region Members

		private AmazonSQSClient _sqsClient;
		private string _awsQueueURL;
		private string _awsAccessKey;
		private int _maxSendToAmazonRetryAttemps = 10;

		#endregion

		#region Properties

		public string EventfulGroup { get; private set; }

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
			ExpandoObject dataToLog = null;
			SendMessageRequest request;

			int numAttempts = 0;
			while (numAttempts < _maxSendToAmazonRetryAttemps)
			{
				try
				{
					numAttempts++;
					dataToLog = buildDynamic(exception, message, eventInfo, deleteAfter);
					request = new SendMessageRequest()
					{
						QueueUrl = _awsQueueURL,
						MessageBody = JsonConvert.SerializeObject(dataToLog)
					};

					_sqsClient.SendMessage(request);
					break;
				}
				catch (Exception ex)
				{
					if (numAttempts >= _maxSendToAmazonRetryAttemps)
					{
						emailError(ex, dataToLog);
					}
					else
					{
						Thread.Sleep(500);
					}
				}
			}
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
			ExpandoObject dataToLog = null;
			SendMessageRequest request;
			
			int numAttempts = 0;
			while (numAttempts < _maxSendToAmazonRetryAttemps)
			{
				try
				{
					numAttempts++;
					dataToLog = buildDynamic(exception, message, eventInfo, deleteAfter);
					request = new SendMessageRequest()
					{
						QueueUrl = _awsQueueURL,
						MessageBody = JsonConvert.SerializeObject(dataToLog)
					};


					_sqsClient.SendMessageAsync(request);
					break;
				}
				catch (Exception ex)
				{
					if (numAttempts >= _maxSendToAmazonRetryAttemps)
					{
						emailError(ex, dataToLog);
					}
					else
					{
						Thread.Sleep(500);
					}
				}
			}
		}


		#endregion

		#region Helpers

		private ExpandoObject buildDynamic(Exception exception, string message, object eventInfo, DateTime? deleteAfter = null)
		{
			return EloggerUtils.BuildDynamic(this.EventfulGroup, exception, message, eventInfo, deleteAfter);
		}

		private void emailError(Exception exception, ExpandoObject dataIntendedToLog)
		{
			NetworkCredential credentials;
			SmtpClient smtpClient;
			string messageBody;

			string host = Properties.Settings.Default.SmtpServer;
			int port = Properties.Settings.Default.SmtpPort;
			bool enableSSL = Properties.Settings.Default.SmtpUseSSL;
			string username = Properties.Settings.Default.SmtpUsername;
			string password = Properties.Settings.Default.SmtpPassword;

			credentials = null;
			if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
				credentials = new NetworkCredential(username, password);

			smtpClient = new SmtpClient()
			{
				Host = host,
				Port = port,
				EnableSsl = enableSSL,
				Credentials = credentials
			};

			messageBody = string.Format("Error occurred trying to post to AmazonSQS:\r\n{0}\r\n\r\nAWS Access Key: {1}\r\nMachineName: {2}\r\n\r\nData Intended to Log:\r\n{3}", exception.ToString(), _awsAccessKey, Environment.MachineName, dataIntendedToLog.Dump());
			//messageBody = "Error occurred trying to post to AmazonSQS:" + Environment.NewLine
			//			  + exception + Environment.NewLine
			//			  + "AWS Access Key: " + _awsAccessKey + Environment.NewLine
			//			  + "Machine Name: " + Environment.MachineName + Environment.NewLine
			//			  + "Data Intended to Log: " + Environment.NewLine
			//			  + dataIntendedToLog.Dump();

			smtpClient.Send(new MailMessage("error@eventful.com", "error@eventul.com", "Error posting to AmazonSQS", messageBody));
		}

		#endregion

	}
}
