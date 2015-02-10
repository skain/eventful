using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using EventfulLogger.LoggingUtils;

namespace eventfulBackend.Utils
{
	public class TimeZoneUtils
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public static readonly TimeZoneInfo CST = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");

		/// <summary>
		/// Safely converts a DateTime from System Local time zone to the supplied User destination time zone. For now, we always use CST for the user's time zone.
		/// </summary>
		/// <param name="srcDateTime"></param>
		/// <param name="destTimeZone"></param>
		/// <returns></returns>
		public static DateTime ToUserLocalTime(DateTime srcDateTime, TimeZoneInfo destTimeZone)
		{
			return ConvertTimeIfNeeded(srcDateTime, TimeZoneInfo.Local, destTimeZone);
		}

		/// <summary>
		/// Safely converts a DateTime from the supplied User's time zone to the System Local time zone. For now, we always use CST for the user's time zone.
		/// </summary>
		/// <param name="srcDateTime"></param>
		/// <param name="srcTimeZone"></param>
		/// <returns></returns>
		public static DateTime ToSystemLocalTime(DateTime srcDateTime, TimeZoneInfo srcTimeZone)
		{
			return ConvertTimeIfNeeded(srcDateTime, srcTimeZone, TimeZoneInfo.Local);
		}

		/// <summary>
		/// If the source time zone and destination time zone are different then this converts the supplied DateTime from the source time zone to the destination time zone. 
		/// If the two time zones are the same then this method returns the date as provided with no processing.
		/// </summary>
		/// <param name="srcDateTime"></param>
		/// <param name="srcTimeZone"></param>
		/// <param name="destTimeZone"></param>
		/// <returns></returns>
		public static DateTime ConvertTimeIfNeeded(DateTime srcDateTime, TimeZoneInfo srcTimeZone, TimeZoneInfo destTimeZone)
		{
			DateTime retVal = srcDateTime;
			if (destTimeZone.Id != srcTimeZone.Id)
			{
				try
				{ 
					retVal = TimeZoneInfo.ConvertTime(srcDateTime, srcTimeZone, destTimeZone);
				}
				catch
				{
					logger.WyzAntError(new { srcDateTime = srcDateTime, srcTimeZone = srcTimeZone, destTimeZone = destTimeZone }, null, "Error converting DateTime.");
					throw;
				}
			}

			return retVal;
		}
	}
}
