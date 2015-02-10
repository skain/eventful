using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eventful.Models
{
	public class ModalDialogButtonModel
	{
		public string Text { get; set; }
		public string CssClass { get; set; }
		public bool ButtonDismissesModal { get; set; }
	}
}