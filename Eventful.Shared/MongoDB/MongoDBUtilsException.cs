using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eventful.Shared.MongoDB
{
	public class MongoDBUtilsException : Exception
	{
		public MongoDBUtilsException() : base()
		{
		}

		public MongoDBUtilsException(string message) : base(message)
		{
		}
	}
}
