using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using EventfulBackend;

namespace Eventful.Models
{
	public class EventfulUserModel
	{

		public string Id { get; set; }
		public string Username { get; set; }
		public List<string> Emails { get; set; }
		public string AsString { get; set; }

		public static string CurrentUserEmail
		{
			get
			{
				return HttpContext.Current.User.Identity.Name;
			}
		}

		public static EventfulUserModel FromEventfulUser(EventfulUser u)
		{
			return Mapper.Map<EventfulUserModel>(u);	
		}

		public EventfulUser AsEventfulUser()
		{
			return Mapper.Map<EventfulUser>(this);
		}

		public static EventfulUserModel GetCurrentUser()
		{
			var eu = EventfulUser.GetByUsername(HttpContext.Current.User.Identity.Name);
			if (eu == null)
			{
				return null;
			}

			return FromEventfulUser(eu);
		}

		public static EventfulUserModel CreateCurrentUser()
		{
			var eu = new EventfulUser
			{
				Username = HttpContext.Current.User.Identity.Name
			};
			eu.Emails.Add(CurrentUserEmail);
			eu.Insert();
			return FromEventfulUser(eu);
		}

		internal static IEnumerable<EventfulUserModel> Search(string searchStr)
		{
			IEnumerable<EventfulUser> users = EventfulUser.Search(searchStr);
			if (users == null)
			{
				return null;
			}

			return Mapper.Map<IEnumerable<EventfulUserModel>>(users);
		}

		internal static EventfulUserModel GetOrCreateCurrentUser()
		{
			var user = EventfulUserModel.GetCurrentUser();
			if (user == null)
			{
				user = EventfulUserModel.CreateCurrentUser();
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
			this.AsEventfulUser().Update();
		}
	}
}