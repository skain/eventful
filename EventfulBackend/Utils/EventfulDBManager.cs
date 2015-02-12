using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using Eventful.Shared.MongoDB;

namespace EventfulBackend.Utils
{
	public static class EventfulDBManager
	{
		public static void ExecuteInContext(Action<MongoDatabase> action)
		{
			MongoDBUtils.ExecuteInContext(Properties.Settings.Default.MongoDBConnectionString, Properties.Settings.Default.MongoDBDatabaseName, action);
		}
	}
}
