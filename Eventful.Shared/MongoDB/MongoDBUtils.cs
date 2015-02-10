using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;

namespace eventful.Shared.MongoDB
{
	public static class MongoDBUtils
	{
		//private static readonly string _mongoDBConnStr = "{Your MongoDB Connection String}";
		////private static readonly string _mongoDBConnStr = "mongodb://wyzant:zaphod42@ds037358.mongolab.com:37358/eventfuldb";
		//private static readonly Lazy<MongoClient> _client = new Lazy<MongoClient>(() => { return new MongoClient(_mongoDBConnStr); }, true);
		//private static readonly Lazy<MongoServer> _server = new Lazy<MongoServer>(() => { return _client.Value.GetServer(); } , true);
		private static readonly Lazy<MongoDBConnectionManager> _connManager = new Lazy<MongoDBConnectionManager>(() => { return new MongoDBConnectionManager(); }, true);

		private static MongoDatabase getMongoDB(string connectionString, string databaseName)
		{
			//var database = _server.Value.GetDatabase("eventful");
			////var database = _server.Value.GetDatabase("eventfuldb");
			//return database;

			return _connManager.Value.GetDatabase(connectionString, databaseName);
		}

		public static void ExecuteInContext(string connectionString, string databaseName, Action<MongoDatabase> action)
		{
			action(getMongoDB(connectionString, databaseName));
		}
	}
}
