using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventfulLogger
{
	public class eventfulLoggerNotConfiguredException : Exception
	{
		public eventfulLoggerNotConfiguredException()
			: base()
		{

		}

		public eventfulLoggerNotConfiguredException(string message)
			: base(message)
		{

		}

		public eventfulLoggerNotConfiguredException(string message, Exception originalException)
			: base(message, originalException)
		{

		}
	}
}
