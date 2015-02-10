using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using eventfulBackend.Utils;
using EventfulLogger;
using MongoDB.Bson;
using Newtonsoft.Json;
using NLog;
using eventful.Shared.Threading;
using EventfulLogger.LoggingUtils;
using EventfulLogger.SharedLoggers;

namespace eventfulQueueProcessor
{
	class Program
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private static LimitedConcurrencyLevelTaskScheduler _taskScheduler = new LimitedConcurrencyLevelTaskScheduler(Properties.Settings.Default.MaxDegreeOfParallelism);
		private static TaskFactory _taskFactory = new TaskFactory(_taskScheduler);
		//private static ELogger eLogger = new ELogger("Offline");
		static void Main(string[] args)
		{
			OfflineLogger.LogApplicationStart();
			int messagesProcessed = 0;
			try
			{
				DateTime start = DateTime.Now;
				while (start.AddMinutes(10) > DateTime.Now)
				{
					messagesProcessed += moveFromSQSToMongo();
				}
			}
			catch (Exception e)
			{
				logger.WyzAntError(e, "Unhandled error in EventQueueProcessor.");
			}

			logger.WyzAntInfo(new { EventType = "eventfulQueueProcessed", MessagesProcessed = messagesProcessed }, null, "eventfulQueueProcessor run completed. Processed {0} messages.", messagesProcessed);
			OfflineLogger.LogApplicationEnd();
		}

		private static int moveFromSQSToMongo()
		{
			AmazonSQSClient client = getSQSClient();
			ReceiveMessageRequest req = new ReceiveMessageRequest();
			req.QueueUrl = Properties.Settings.Default.eventfulQueueUrl;
			req.MaxNumberOfMessages = 10;

			int messagesProcessed = 0;

			List<Task> tasks = new List<Task>();
			ReceiveMessageResponse resp = null;

			eventfulDBManager.ExecuteInContext((db) =>
			{
				var eventsCollection = db.GetCollection("eventfulEvents");
				do
				{
					resp = client.ReceiveMessage(req);
					foreach (var m in resp.Messages)
					{
						tasks.Add(_taskFactory.StartNew(() =>
						{
							try
							{
								//logger.Debug("Message received: {0}", m.ToJson());
								var msgObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(m.Body);

								eventsCollection.Insert(new BsonDocument(msgObj));

								//logger.Debug("Message inserted in Mongo.");

								messagesProcessed++;
							}
							catch (Exception e)
							{
								logger.WyzAntError(e, "Error processing eventful message off of the queue: {0}", m.Body);
							}
							finally
							{
								deleteSQSMessage(client, m);
							}
						}));
					}
				} while (resp.Messages.Count > 0);

			});

			Task.WaitAll(tasks.ToArray());

			return messagesProcessed;
		}

		private static void deleteSQSMessage(AmazonSQSClient client, Message m)
		{
			DeleteMessageRequest delReq = new DeleteMessageRequest();
			delReq.QueueUrl = Properties.Settings.Default.eventfulQueueUrl;
			delReq.ReceiptHandle = m.ReceiptHandle;

			client.DeleteMessageAsync(delReq);
			//logger.Debug("Async delete request sent for message.");
			//DeleteMessageResponse delResp = client.DeleteMessage(delReq);
			//logger.Debug("Message deleted: {0}", delResp.ToJson());
		}

		private static void simpleSQSTest()
		{
			AmazonSQSClient client = getSQSClient();
			SendMessageRequest req = new SendMessageRequest();
			req.QueueUrl = Properties.Settings.Default.eventfulQueueUrl;
			req.MessageBody = "this is a test";
			//SendMessageResponse resp = client.SendMessage(req);
			//logger.Debug("Response: {0}", resp.SendMessageResult.ToJson());

			ReceiveMessageRequest rec = new ReceiveMessageRequest();
			rec.QueueUrl = req.QueueUrl;
			ReceiveMessageResponse recResp = client.ReceiveMessage(rec);

			foreach (var m in recResp.Messages)
			{
				//logger.Debug("Message received: {0}", m.ToJson());

				DeleteMessageRequest delReq = new DeleteMessageRequest();
				delReq.QueueUrl = req.QueueUrl;
				delReq.ReceiptHandle = m.ReceiptHandle;

				DeleteMessageResponse delResp = client.DeleteMessage(delReq);
				//logger.Debug("Message deleted: {0}", delResp.ToJson());
			}

		}

		private static AmazonSQSClient getSQSClient()
		{
			AmazonSQSConfig config = new AmazonSQSConfig();
			config.ServiceURL = Properties.Settings.Default.AWSServiceUrl;
			AmazonSQSClient client = new AmazonSQSClient(Properties.Settings.Default.AWSAccessKey, Properties.Settings.Default.AWSSecretKey, config);
			return client;
		}
	}
}
