using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;

namespace eventful.Shared.MongoDB
{
	public class MongoDBConnectionManager
	{
		private Dictionary<string, MongoDatabase> _databases = new Dictionary<string, MongoDatabase>();

		public MongoDatabase GetDatabase(string connectionString, string databaseName)
		{
			string dbKey = buildDBKey(connectionString, databaseName);
			if (!_databases.ContainsKey(dbKey))
			{
				lock (this)
				{
					if (!_databases.ContainsKey(dbKey))
					{
						addNewDatabaseInstance(dbKey, connectionString, databaseName);
					}
				}
			}

			if (!_databases.ContainsKey(dbKey))
			{
				throw new MongoDBUtilsException(string.Format("Error. Unable to retrive MongoDb instance for ConnectionString: {0}\r\nDatabaseName: {1}\r\ndbKey: {2}", connectionString, databaseName, dbKey));
			}

			return _databases[dbKey];
		}

		private void addNewDatabaseInstance(string dbKey, string connectionString, string databaseName)
		{
			MongoClient client = new MongoClient(connectionString);
			MongoServer server = client.GetServer();
			MongoDatabase db = server.GetDatabase(databaseName);
			
			_databases.Add(dbKey, db);
		}

		private string buildDBKey(string connectionString, string databaseName)
		{
			return string.Concat(connectionString, "_", databaseName);
		}
	}
}
