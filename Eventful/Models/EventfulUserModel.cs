using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using eventfulBackend;

namespace eventful.Models
{
	public class eventfulUserModel
	{

		public string Id { get; set; }
		public string Username { get; set; }
		public List<string> Emails { get; set; }
		public string AsString { get; set; }

		public static string CurrentUserEmail
		{
			get
			{
				//return HttpContext.Current.Profile["WyzantEmployeeEmail"].ToString();
				return HttpContext.Current.User.Identity.Name;
			}
		}

		public static eventfulUserModel FromeventfulUser(eventfulUser u)
		{
			return Mapper.Map<eventfulUserModel>(u);	
		}

		public eventfulUser AseventfulUser()
		{
			return Mapper.Map<eventfulUser>(this);
		}

		public static eventfulUserModel GetCurrentUser()
		{
			var eu = eventfulUser.GetByUsername(HttpContext.Current.User.Identity.Name);
			if (eu == null)
			{
				return null;
			}

			return FromeventfulUser(eu);
		}

		public static eventfulUserModel CreateCurrentUser()
		{
			var eu = new eventfulUser
			{
				Username = HttpContext.Current.User.Identity.Name
			};
			eu.Emails.Add(CurrentUserEmail);
			eu.Insert();
			return FromeventfulUser(eu);
		}

		internal static IEnumerable<eventfulUserModel> Search(string searchStr)
		{
			IEnumerable<eventfulUser> users = eventfulUser.Search(searchStr);
			if (users == null)
			{
				return null;
			}

			return Mapper.Map<IEnumerable<eventfulUserModel>>(users);
		}

		internal static eventfulUserModel GetOrCreateCurrentUser()
		{
			var user = eventfulUserModel.GetCurrentUser();
			if (user == null)
			{
				user = eventfulUserModel.CreateCurrentUser();
			}
			return user;
		}

		internal void UpdateEmail(string oldEmail, string newEmail)
		{
			Emails.Remove(oldEmail);
			Emails.Add(newEmail);
			this.Update();
			KlaxonModel.UpdateSubscriberEmail(oldEmail, newEmail);
		}

		public bool EmailIsPrimary(string email)
		{
			return email == CurrentUserEmail;
		}

		internal void Update()
		{
			this.AseventfulUser().Update();
		}
	}
}