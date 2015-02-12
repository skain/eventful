using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EventfulBackend.Utils;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace EventfulBackend
{
	public class EventfulUser : MongoDBEntity
	{
		private static readonly Regex _fromStringRE = new Regex(@"(?<username>.*?) <(?<email>.*?)> - (?<id>.*)");
		public string Username { get; set; }
		public List<string> Emails { get; set; }

		public EventfulUser()
		{
			Emails = new List<string>();
		}
		public string AsString
		{
			get
			{
				return string.Concat(Username, " <", Emails.First(), "> - ", Id);
			}
		}

		public static EventfulUser FromString(string str)
		{
			if (!_fromStringRE.IsMatch(str))
			{
				throw new ApplicationException(string.Format("Error. Unable to parse User string: {0}", str));
			}

			var m = _fromStringRE.Match(str);
			EventfulUser eu = new EventfulUser
			{
				Id = m.Groups["id"].Value,
				Username = m.Groups["username"].Value
			};
			eu.Emails.Add(m.Groups["email"].Value);
			return eu;
		}

		public void Insert()
		{
			EventfulDBManager.ExecuteInContext((db) =>
			{
				var dbUsers = getCollection(db);
				dbUsers.Insert(this);
			});
		}

		public static EventfulUser GetById(string id)
		{
			EventfulUser eu = null;
			EventfulDBManager.ExecuteInContext((db) =>
			{
				var users = getCollection(db);
				var query = from u in users.AsQueryable()
							where u.Id == id
							select u;

				eu = query.FirstOrDefault();
			});

			return eu;
		}

		public static IEnumerable<EventfulUser> GetByIds(IEnumerable<string> ids)
		{
			IEnumerable<EventfulUser> eus = null;
			EventfulDBManager.ExecuteInContext((db) =>
			{
				var users = getCollection(db);
				var query = from u in users.AsQueryable()
							where ids.Contains(u.Id)
							select u;

				eus = query.ToArray();
			});

			return eus;
		}

		public static EventfulUser GetByUsername(string username)
		{
			EventfulUser eu = null;
			EventfulDBManager.ExecuteInContext((db) =>
			{
				var users = getCollection(db);
				var query = from u in users.AsQueryable()
							where u.Username == username
							select u;

				eu = query.FirstOrDefault();
			});

			return eu;
		}

		private static MongoCollection<EventfulUser> getCollection(MongoDatabase db)
		{
			return db.GetCollection<EventfulUser>("eventfulUsers");
		}

		public static IEnumerable<EventfulUser> Search(string searchStr)
		{
			IEnumerable<EventfulUser> users = null;
			EventfulDBManager.ExecuteInContext((db) =>
			{
				var usersCollection = getCollection(db);
				var query = from u in usersCollection.AsQueryable()
							where u.Username.Contains(searchStr)
							|| (u.Emails != null && u.Emails.Contains(searchStr))
							select u;

				users = query.ToArray();
			});

			return users;
		}

		public void Update()
		{
			EventfulDBManager.ExecuteInContext((db) =>
				{
					var users = getCollection(db);
					users.Save(this);
				});
		}
	}
}
