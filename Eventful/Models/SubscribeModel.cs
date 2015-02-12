using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EventfulBackend;

namespace Eventful.Models
{
	public class SubscribeModel
	{
		public IEnumerable<string> UserEmails { get; private set; }
		public string KlaxonIDsToSubscribeTo { get; set; }
		public string SelectedEmail { get; set; }

		public SubscribeModel(string klaxonIDsToSubscribeTo)
		{
			KlaxonIDsToSubscribeTo = klaxonIDsToSubscribeTo;
			EventfulUserModel eum = EventfulUserModel.GetOrCreateCurrentUser();
			UserEmails = eum.Emails;
			SelectedEmail = null;
		}
	}
}