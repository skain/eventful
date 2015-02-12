using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eventful.Shared.MongoDB
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
