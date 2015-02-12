using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Eventful.Models;

namespace Eventful.Controllers
{
	[Authorize]
	public class UsersController : Controller
    {
		public ActionResult Account()
		{
			var eum = EventfulUserModel.GetOrCreateCurrentUser();
			return View(eum);
		}
        //
        // GET: /eventful/Users/

        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /eventful/Users/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /eventful/Users/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /eventful/Users/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        
        //
        // GET: /eventful/Users/Edit/5
 
        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /eventful/Users/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here
 
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /eventful/Users/Delete/5
 
        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /eventful/Users/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here
 
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

		[HttpPost]
		public ActionResult Search(string searchStr)
		{
			if (string.IsNullOrWhiteSpace(searchStr))
			{
				throw new ApplicationException("Error. SearchStr cannot be null or whitespace.");
			}
			IEnumerable<EventfulUserModel> users = EventfulUserModel.Search(searchStr);
			return Json(users);
		}

		[HttpPost]
		public ActionResult EditEmail(string newEmail, string oldEmail)
		{
			var u = EventfulUserModel.GetCurrentUser();
			if (u.Emails.Contains(oldEmail))
			{
				u.UpdateEmail(oldEmail, newEmail);
			}
			return RedirectToAction("Account");
		}

		[HttpPost]
		public ActionResult AddEmail(string newEmail)
		{
			var u = EventfulUserModel.GetCurrentUser();
			u.Emails.Add(newEmail);
			u.Update();
			return RedirectToAction("Account");
		}

		[HttpPost]
		public ActionResult DeleteEmail(string deleteEmail)
		{
			var u = EventfulUserModel.GetCurrentUser();
			if (u.Emails.Contains(deleteEmail))
			{
				u.Emails.Remove(deleteEmail);
				u.Update();
				KlaxonModel.DeleteSubscriberEmail(deleteEmail);
			}

			return RedirectToAction("Account");
		}
    }
}
