using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using eventfulBackend.eventfulReporting;
using eventfulBackend.Klaxon;
using eventfulBackend.Utils;
using NLog;
using eventful.Shared.SlackAPI;

namespace eventfulTestApp
{
	class Program
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		static void Main(string[] args)
		{
			//basicTest();
			testReport();
		}

		private static void testReport()
		{
			eventfulReport r = eventfulReport.GetById("53a88c1a9ad86605736c6529");
			string table = r.GetAsSlackMessage(DateTime.Now.AddDays(-2), DateTime.Now);
			SlackClient client = new SlackClient("wyzant", "IcdeMce3HAKGndgRlgz5JXVr");
			SlackMessage msg = new SlackMessage { username = "KlaxonProcessor" };
			SlackMessageAttachment att = new SlackMessageAttachment { fallback = "Alert!", pretext = "Alert!", text = table };
			msg.AddAttachment(att);
			client.Post(msg);
		}

		private static void basicTest()
		{
			SlackClient client = new SlackClient("wyzant", "IcdeMce3HAKGndgRlgz5JXVr");
			SlackMessage msg = new SlackMessage { username = "KlaxonProcessor" };
			SlackMessageAttachment att = new SlackMessageAttachment { fallback = "Link to google", text = "*title* value\r\n*title2* value2" };
			att.AddField(new SlackMessageAttachmentField { @short = true, title = "Title", value = "Value" });
			msg.AddAttachment(att);
			//msg.text = "*title* value\r\n*title2* value2";
			client.Post(msg);
		}

	}
}
