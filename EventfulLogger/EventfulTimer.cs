using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventfulLogger
{
	public class EventfulTimer : IDisposable
	{
		private ELogger _eLogger = null;
		private Stopwatch _stopwatch = null;
		private string _message = null;
		private DateTime? _deleteAfter = null;
		private bool _logAsync = true;

		public EventfulTimer(string eventfulGroup, DateTime? deleteAfter = null, bool logAsync = true)
		{
			_eLogger = new ELogger(eventfulGroup);
			_deleteAfter = deleteAfter;
			_logAsync = logAsync;
			_stopwatch = new Stopwatch();
			_stopwatch.Start();
		}

		public EventfulTimer(string eventfulGroup, TimeSpan deleteAfter, bool logAsync = true) : this(eventfulGroup, DateTime.Now + deleteAfter, logAsync)
		{

		}

		public void SetMessage(string message)
		{
			_message = message;
		}
		#region IDisposable Members

		public void Dispose()
		{
			_stopwatch.Stop();
			if (_logAsync)
			{
				_eLogger.LogEventAsync(_message, new { EllapsedTimeInMS = _stopwatch.ElapsedMilliseconds }, _deleteAfter);
			}
			else
			{
				_eLogger.LogEvent(_message, new { EllapsedTimeInMS = _stopwatch.ElapsedMilliseconds }, _deleteAfter);
			}
		}

		#endregion
	}
}
