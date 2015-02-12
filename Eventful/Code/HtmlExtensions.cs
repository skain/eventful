using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Eventful.Code
{
	public static class HtmlExtensions
	{
		public static IDisposable BootsrapModal(this HtmlHelper helper, string id, string cssClass, string title, bool includeCloseButton)
		{
			return new ModalHelper(helper, id, cssClass, title, includeCloseButton);
		}
	}
}