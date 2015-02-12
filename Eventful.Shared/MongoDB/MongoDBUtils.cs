using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;

namespace Eventful.Shared.MongoDB
{
	public static class MongoDBUtils
	{
		private static readonly Lazy<MongoDBConnectionManager> _connManager = new Lazy<MongoDBConnectionManager>(() => { return new MongoDBConnectionManager(); }, true);

		private static MongoDatabase getMongoDB(string connectionString, string databaseName)
		{
			return _connManager.Value.GetDatabase(connectionString, databaseName);
		}

		public static void ExecuteInContext(string connectionString, string databaseName, Action<MongoDatabase> action)
		{
			action(getMongoDB(connectionString, databaseName));
		}
	}
}
