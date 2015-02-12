using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eventful.Shared.SlackAPI
{
	public class SlackMessageAttachment
	{
		private List<SlackMessageAttachmentField> _fields = new List<SlackMessageAttachmentField>();
		public string fallback { get; set; }
		public string pretext { get; set; }
		public string text { get; set; }
		public string color { get; set; }
		public string[] mrkdwn_in
		{
			get
			{
				return new string[] { "text", "title", "pretext", "fallback" };
			}
		}
		public IEnumerable<SlackMessageAttachmentField> fields
		{
			get
			{
				return _fields.ToArray();
			}
		}

		public void AddField(SlackMessageAttachmentField field)
		{
			_fields.Add(field);
		}
	}
}
