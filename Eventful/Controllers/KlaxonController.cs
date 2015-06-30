using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EventfulBackend.Klaxon;
using NLog;
using Eventful.Models;
using EventfulLogger.LoggingUtils;

namespace Eventful.Controllers
{
	//[Authorize]
	public class KlaxonController : Controller
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		//
		// GET: /eventful/Klaxon/

		public ActionResult Index()
		{
			var ks = KlaxonModel.GetAll();
			return View(ks);
		}

		//
		// GET: /eventful/Klaxon/Details/5

		public ActionResult Details(int id)
		{
			return View();
		}

		//
		// GET: /eventful/Klaxon/Create

		public ActionResult Create()
		{
			return View();
		}

		//
		// GET: /eventful/Klaxon/Edit/5

		public ActionResult Edit(string id)
		{
			return View();
		}


		//
		// GET: /eventful/Klaxon/Delete/5

		//public ActionResult Delete(string id)
		//{
		//	KlaxonModel.DeleteById(id);
		//	return RedirectToAction("Index");
		//}

		////
		//// POST: /eventful/Klaxon/Delete/5

		//[HttpPost]
		//public ActionResult Delete(int id, FormCollection collection)
		//{
		//	try
		//	{
		//		// TODO: Add delete logic here

		//		return RedirectToAction("Index");
		//	}
		//	catch
		//	{
		//		return View();
		//	}
		//}

		[HttpPost]
		public ActionResult Test(KlaxonModel km)
		{
			try
			{
				km.RunTest(false);
				return Json(km);
			}
			catch (Exception e)
			{
				logger.EventfulError(e, "Error running test.");
				return Json(km);
			}
		}

		public ActionResult RunTest(string id)
		{
			try
			{
				KlaxonModel km = KlaxonModel.GetById(id);
				km.RunTest(true);
			}
			catch (Exception e)
			{
				logger.EventfulError(e, "Error running test.");
			}

			return RedirectToAction("Index");
		}

		public ActionResult ToggleSubscription(string id)
		{
			KlaxonModel km = KlaxonModel.GetById(id);
			bool isSubbed = km.ToggleSubscriptionForCurrentUser();
			return RedirectToAction("Index");
		}

		public ActionResult ResetNextCheck(string id)
		{
			KlaxonModel km = KlaxonModel.GetById(id);
			km.NextCheckAt = new DateTime(1970, 1, 1);
			km.Update(false);

			return RedirectToAction("Index");
		}

		public ActionResult SubscribeDialog(string idsStr)
		{
			//ensure we have created the user
			EventfulUserModel.GetOrCreateCurrentUser();

			string[] ids = idsStr.Split(',');
			SubscribeModel m = new SubscribeModel(idsStr);
			if (m.UserEmails.Count() == 1)
			{
				foreach (string id in ids)
				{
					KlaxonModel km = KlaxonModel.GetById(id);
					km.SubscriberEmails.Add(m.UserEmails.First());
					km.Update(false);
				}
				return Json(new { Subscribed = true }, JsonRequestBehavior.AllowGet);
			}
			else
			{
				return PartialView(m);
			}
		}

		public ActionResult Unsubscribe(string idsStr)
		{
			EventfulUserModel eum = EventfulUserModel.GetOrCreateCurrentUser();
			string[] ids = idsStr.Split(',');
			foreach (string id in ids.Where(i => !string.IsNullOrWhiteSpace(i)))
			{
				KlaxonModel km = KlaxonModel.GetById(id);
				km.SubscriberEmails.RemoveAll(e => eum.Emails.Contains(e));
				km.Update(false);
			}
			return RedirectToAction("Index");
		}

		public ActionResult Subscribe(string email, string idsStr)
		{
			string[] ids = idsStr.Split(',');
			foreach (string id in ids.Where(i => !string.IsNullOrWhiteSpace(i)))
			{
				KlaxonModel km = KlaxonModel.GetById(id);
				km.SubscriberEmails.Add(email);
				km.Update(false);
			}
			return RedirectToAction("Index");
		}
	}
}
