using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eventful.Shared.Strings
{
	public static class StringUtils
	{
		public static string SimpleTruncate(string str, int textLength, string append = null)
		{
			if (string.IsNullOrEmpty(str))
			{
				return string.Empty;
			}

			if (str.Length < textLength)
			{
				return str;
			}

			return string.Format("{0}{1}", str.Substring(0, textLength), append);
		}


	}
}
