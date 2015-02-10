using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eventfulBackend.Klaxon;
using EventfulLogger.LoggingUtils;
using NLog;
using eventfulBackend.eventfulReporting;
using System.Net.Mail;
using System.Web;
using EventfulLogger.SharedLoggers;
using eventfulBackend.Utils;
using System.Threading;
using eventful.Shared.SlackAPI;
using eventful.Shared.Strings;

namespace EventfulKlaxonProcessor
{
	class Program
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		static void Main(string[] args)
		{
			OfflineLogger.LogApplicationStart();
			DateTime start = DateTime.Now;
			while (start.AddMinutes(10) > DateTime.Now)
			{
				try
				{
					processKlaxons();
				}
				catch (Exception e)
				{
					logger.WyzAntError(e, "Unhandled Error during EventfulKlaxonProcessor run.");
				}

				Thread.Sleep(10000);
			}
			OfflineLogger.LogApplicationEnd();
		}

		private static void sentTestEmail(string emailTo)
		{
			MailMessage mm = new MailMessage("klaxon@wyzant.com", emailTo);
			mm.Body = "This is a test";
			sendMailMessage(mm);
		}

		private static void processKlaxons()
		{
			IEnumerable<Klaxon> klaxons = Klaxon.GetAllDueForTesting();
			logger.WyzAntDebug("Found {0} Klaxons to process", klaxons.Count());
			foreach (var k in klaxons)
			{
				string subject = string.Format("Klaxon test failed: {0}", k.Name);
				string body = "This Klaxon fired but an error was encountered during the formatting of this email. Please log into eventful to investigate the issue further.";
				try
				{
					k.RunTest(true);
					if (k.TestResult)
					{
						eventfulReport r = k.GetReport();
						string reportLink = string.Empty;
						string resultsTable = string.Empty;
						string resultsForSlack = string.Empty;
						if (r != null)
						{
							try
							{
								reportLink = buildKlaxonReportLink(k, r);
								resultsTable = r.GetAsHTMLTables(k.AggregationResult.StartTime, k.AggregationResult.EndTime);
								resultsForSlack = r.GetAsSlackMessage(k.AggregationResult.StartTime, k.AggregationResult.EndTime);
							}
							catch (Exception e)
							{
								logger.WyzAntError(e, "Error pulling or formatting Klaxon report.");
							}
						}
						else
						{
							reportLink = buildKlaxonAggregateLink(k);
						}

						string statement = string.Empty;

						try
						{
							subject = string.Format("Klaxon test failed: {0} at {1}", k.Name, TimeZoneUtils.ToUserLocalTime(DateTime.Now, TimeZoneUtils.CST).ToString());
							statement = string.Format("<p>The statement, '{0}' '{1}' '{2}' '{3}'(s) evaluates to true.</p>", k.NumMatched, k.ComparisonOperator, k.ComparisonThreshold, k.FieldToAggregate);
							string bodyPre = "<!DOCTYPE html><html><head><meta content='telephone=no' name='format-detection'></head><body style='max-width: 600px; font-family: helvetica, arial, sans-serfif; font-size: 12px; -webkit-text-size-adjust: 100%;'>";
							string bodyPost = "</body></html>";

							body = string.Format("{0}<p>The Klaxon '{1}' has been triggered.</p>{2}{3}<hr />{4}{5}", bodyPre, k.Name, statement, reportLink, resultsTable, bodyPost);
						}
						catch (Exception e)
						{
							logger.WyzAntError(e, "Error formatting Klaxon email.");
						}

						foreach (var email in k.SubscriberEmails)
						{
							try
							{
								MailMessage mm = new MailMessage("klaxon@wyzant.com", email, subject, body);
								mm.IsBodyHtml = true;
								sendMailMessage(mm);
							}
							catch (Exception e)
							{
								logger.WyzAntError(e, "Error sending Klaxon email to: '{0}'", email);
							}
						}

						string reportHref = buildKlaxonReportHref(k, r) ?? buildKlaxonAggregateHref(k);
						sendToSlack(subject, statement, reportHref, resultsForSlack);
					}
					logger.WyzAntDebug("Processed Klaxon: {0}.", k.Name);
				}
				catch (Exception e)
				{
					logger.WyzAntError(e, "Error processing Klaxon: {0}", k == null ? "NULL" : k.Name);
				}
			}
		}

		private static void sendToSlack(string subject, string statement, string reportHref, string resultsForSlack)
		{
			subject = HtmlUtils.StripHtml(subject, false);
			statement = HtmlUtils.StripHtml(statement, false);
			reportHref = string.IsNullOrWhiteSpace(reportHref) ? string.Empty : string.Format("\r\nClick <{0}|here> to view report\r\n", reportHref);
			resultsForSlack = string.Concat(reportHref, resultsForSlack);
			SlackClient client = new SlackClient("wyzant", "IcdeMce3HAKGndgRlgz5JXVr");
			SlackMessage msg = new SlackMessage { username = "KlaxonProcessor" };
			msg.text = subject;
			SlackMessageAttachment att = new SlackMessageAttachment { fallback = subject, pretext = statement, text = resultsForSlack };
			msg.AddAttachment(att);
			client.Post(msg);
		}

		private static void sendMailMessage(MailMessage mm)
		{
			SmtpClient smtpClient = new SmtpClient();
			smtpClient.Host = Properties.Settings.Default.SmtpServer;
			smtpClient.Port = Properties.Settings.Default.SmtpPort;
			smtpClient.EnableSsl = false;
			smtpClient.Credentials = new System.Net.NetworkCredential(Properties.Settings.Default.SmtpUsername, Properties.Settings.Default.SmtpPassword);

			smtpClient.Send(mm);
		}

		private static string buildKlaxonReportLink(Klaxon k, eventfulReport r)
		{
			string reportLink = null;
			string href = buildKlaxonReportHref(k, r);
			reportLink = string.Format("<p>Click <a href='{0}'>here</a> to view the {1} report for more information.</p>", href, r.Title);
			return reportLink;
		}

		private static string buildKlaxonReportHref(Klaxon k, eventfulReport r)
		{
			if (r == null)
			{
				return null;
			}
			string qs = getTimespanQSFromKlaxon(k, true);
			string href = string.Format("http://eventful.wyzant.com/reports/view/{0}?{1}", r.Id, qs);
			return href;
		}

		private static string buildKlaxonAggregateLink(Klaxon k)
		{
			string aggLink = buildKlaxonAggregateHref(k);
			return string.Format("<p>Click <a href='{0}'>here</a> to view the aggregate query for more information.</p>", aggLink);
		}

		private static string buildKlaxonAggregateHref(Klaxon k)
		{
			string aggLink = "http://eventful.wyzant.com/aggregate?{0}&EqlQuery={1}&AggregateOperator={2}&AggregateFieldName={3}";
			string ts = getTimespanQSFromKlaxon(k, false);
			string aggOp = encodeValueForURL(k.AggregateOperation);
			string aggField = encodeValueForURL(k.FieldToAggregate);
			string eql = encodeValueForURL(k.EqlQuery);
			aggLink = string.Format(aggLink, ts, eql, aggOp, aggField);
			return aggLink;
		}

		private static string getTimespanQSFromKlaxon(Klaxon k, bool linkIsReportLink)
		{
			string startTimeTBName = "StartTimeTB";
			string endTimeTBName = "EndTimeTB";
			if (!linkIsReportLink)
			{
				startTimeTBName = "RequestedStartTime";
				endTimeTBName = "RequestedEndTime";
			}
			DateTime startTime = TimeZoneUtils.ToUserLocalTime(k.AggregationResult.StartTime, TimeZoneUtils.CST);
			DateTime endTime = TimeZoneUtils.ToUserLocalTime(k.AggregationResult.EndTime, TimeZoneUtils.CST);
			string qs = string.Format("{0}={1}&{2}={3}", startTimeTBName, encodeValueForURL(startTime.ToString()), endTimeTBName, encodeValueForURL(endTime.ToString()));
			return qs;
		}

		private static string encodeValueForURL(string str)
		{
			return HttpUtility.UrlEncode(str ?? string.Empty).Replace("+", "%20");
		}
	}
}
