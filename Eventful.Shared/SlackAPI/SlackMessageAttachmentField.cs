using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eventful.Shared.SlackAPI
{
	public class SlackMessageAttachmentField
	{
		public string title { get; set; }
		public string value { get; set; }
		public bool @short { get; set; }

	}
}
