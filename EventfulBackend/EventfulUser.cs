using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using eventfulBackend.Utils;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace eventfulBackend
{
	public class eventfulUser : MongoDBEntity
	{
		private static readonly Regex _fromStringRE = new Regex(@"(?<username>.*?) <(?<email>.*?)> - (?<id>.*)");
		public string Username { get; set; }
		public List<string> Emails { get; set; }

		public eventfulUser()
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

		public static eventfulUser FromString(string str)
		{
			if (!_fromStringRE.IsMatch(str))
			{
				throw new ApplicationException(string.Format("Error. Unable to parse User string: {0}", str));
			}

			var m = _fromStringRE.Match(str);
			eventfulUser eu = new eventfulUser
			{
				Id = m.Groups["id"].Value,
				Username = m.Groups["username"].Value
			};
			eu.Emails.Add(m.Groups["email"].Value);
			return eu;
		}

		public void Insert()
		{
			eventfulDBManager.ExecuteInContext((db) =>
			{
				var dbUsers = getCollection(db);
				dbUsers.Insert(this);
			});
		}

		public static eventfulUser GetById(string id)
		{
			eventfulUser eu = null;
			eventfulDBManager.ExecuteInContext((db) =>
			{
				var users = getCollection(db);
				var query = from u in users.AsQueryable()
							where u.Id == id
							select u;

				eu = query.FirstOrDefault();
			});

			return eu;
		}

		public static IEnumerable<eventfulUser> GetByIds(IEnumerable<string> ids)
		{
			IEnumerable<eventfulUser> eus = null;
			eventfulDBManager.ExecuteInContext((db) =>
			{
				var users = getCollection(db);
				var query = from u in users.AsQueryable()
							where ids.Contains(u.Id)
							select u;

				eus = query.ToArray();
			});

			return eus;
		}

		public static eventfulUser GetByUsername(string username)
		{
			eventfulUser eu = null;
			eventfulDBManager.ExecuteInContext((db) =>
			{
				var users = getCollection(db);
				var query = from u in users.AsQueryable()
							where u.Username == username
							select u;

				eu = query.FirstOrDefault();
			});

			return eu;
		}

		private static MongoCollection<eventfulUser> getCollection(MongoDatabase db)
		{
			return db.GetCollection<eventfulUser>("eventfulUsers");
		}

		public static IEnumerable<eventfulUser> Search(string searchStr)
		{
			IEnumerable<eventfulUser> users = null;
			eventfulDBManager.ExecuteInContext((db) =>
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
			eventfulDBManager.ExecuteInContext((db) =>
				{
					var users = getCollection(db);
					users.Save(this);
				});
		}
	}
}
