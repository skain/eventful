using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using RestSharp;

namespace Eventful.Shared.SlackAPI
{
	/// <summary>
	/// The SlackClient class encapsulates communication protocols with slack.com via their API. The plan for this class is to implement SlackAPI calls on an as-needed basis.
	/// </summary>
	public class SlackClient
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private string resource = "/services/hooks/incoming-webhook?token={0}";
		public string TeamName { get; set; }

		public RestClient _restClient { get; set; }

		/// <summary>
		/// The teamName and token parameters are retrieved from the app.config if left null.
		/// </summary>
		/// <param name="teamName"></param>
		/// <param name="token"></param>
		/// <param name="restClient"></param>
		public SlackClient(string teamName = null, string token = null, RestClient restClient = null)
		{
			//JsConfig.EmitLowercaseUnderscoreNames = true;
			//JsConfig.IncludeNullValues = false;
			//JsConfig.PropertyConvention = PropertyConvention.Lenient;

			TeamName = teamName ?? Properties.Settings.Default.SlackAPITeamName;
			token = token ?? Properties.Settings.Default.SlackAPIToken;
			if (token == "Unset")
			{
				throw new ApplicationException("Error. SlackAPIToken must be passed to constructor or set in app.config");
			}

			resource = string.Format(resource, token);
			_restClient = restClient ?? new RestClient(string.Format("https://{0}.slack.com", teamName));
		}

		/// <summary>
		/// Post a SlackMessage to the SlackAPI.
		/// </summary>
		/// <param name="slackMessage"></param>
		/// <returns></returns>
		public bool Post(SlackMessage slackMessage)
		{
			var request = new RestRequest(resource, Method.POST);
			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.NullValueHandling = NullValueHandling.Ignore;
			string payload = JsonConvert.SerializeObject(slackMessage);
			request.AddParameter("payload", payload);
			IRestResponse response = null;
			try
			{
				response = _restClient.Execute(request);
				logger.Debug("Response from SlackAPI:\r\n\r\n{0}", JsonConvert.SerializeObject(response));
				return response.StatusCode == HttpStatusCode.OK;
			}
			catch (Exception e)
			{
				logger.Error("Error posting to SlackAPI\r\nResponse:{0}\r\n{1}", JsonConvert.SerializeObject(response), e.ToString());
				return false;
			}
		}
	}
}
