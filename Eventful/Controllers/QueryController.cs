using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Eventful.Models;

namespace Eventful.Controllers
{
	//[Authorize]
	public class QueryController : Controller
    {
        //
        // GET: /eventful/Query/

        public ActionResult Index()
        {
            return View(new EventfulQueryModel());
        }

		[HttpPost]
		public ActionResult Index(EventfulQueryModel eqm)
		{
			eqm.ExecuteQuery();
			return Json(eqm);
		}

    }
}
