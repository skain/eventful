using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Eventful.Controllers
{
	[Authorize]
	public class HelpController : Controller
    {
        //
        // GET: /eventful/Help/

        public ActionResult Index()
        {
            return View();
        }

    }
}
