using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventfulLogger
{
	public class EventfulLoggerNotConfiguredException : Exception
	{
		public EventfulLoggerNotConfiguredException()
			: base()
		{

		}

		public EventfulLoggerNotConfiguredException(string message)
			: base(message)
		{

		}

		public EventfulLoggerNotConfiguredException(string message, Exception originalException)
			: base(message, originalException)
		{

		}
	}
}
