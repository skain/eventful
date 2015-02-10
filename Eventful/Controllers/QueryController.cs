using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eventful.Models;

namespace eventful.Controllers
{
	[Authorize]
	public class QueryController : Controller
    {
        //
        // GET: /eventful/Query/

        public ActionResult Index()
        {
            return View(new eventfulQueryModel());
        }

		[HttpPost]
		public ActionResult Index(eventfulQueryModel eqm)
		{
			eqm.ExecuteQuery();
			return Json(eqm);
		}

    }
}
