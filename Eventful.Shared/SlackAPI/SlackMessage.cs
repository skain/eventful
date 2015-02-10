using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eventful.Shared.SlackAPI
{
	public class SlackMessage
	{
		private List<SlackMessageAttachment> _attachments = new List<SlackMessageAttachment>();
		public string text { get; set; }
		public string channel { get; set; }
		public string username { get; set; }
		public string icon_emoji { get; set; }
		public IEnumerable<SlackMessageAttachment> attachments 
		{ 		
			get
			{
				return _attachments.ToArray();
			} 		
		}

		public void AddAttachment(SlackMessageAttachment att)
		{
			_attachments.Add(att);
		}
	}
}
