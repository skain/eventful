using System;
using System.Text;
using Eventful.Shared.Strings;

namespace Eventful.Shared.ExtensionMethods
{
	public static class StringExtensions
	{
		/// <summary>
		/// A 'safe' truncate function. You can call this on a string of any length 
		/// or even a null or empty string. If the string is longer than the 
		/// textLength then the string will be truncated to textLength. If not then 
		/// the string is returned unmodified. The append parameter will add a string
		/// to the end of the truncated one, like '...' for example.
		/// </summary>
		/// <param name="str"></param>
		/// <param name="textLength"></param>
		/// <returns></returns>
		public static string SimpleTruncate(this string str, int textLength, string append = null)
		{
			return StringUtils.SimpleTruncate(str, textLength, append);
		}

		/// <summary>
		/// Allows for a String.Replace with a StringComparison specified.  User with StringComparison.OrdinalIgnoreCase to do a a case-insensitve Replace.
		/// </summary>
		/// <param name="original"></param>
		/// <param name="pattern"></param>
		/// <param name="replacement"></param>
		/// <param name="comparisonType"></param>
		/// <returns></returns>
		static public string Replace(this string original, string pattern, string replacement, StringComparison comparisonType)
		{
			return Replace(original, pattern, replacement, comparisonType, -1);
		}

		/// <summary>
		/// Allows for a String.Replace with a StringComparison specified.  User with StringComparison.OrdinalIgnoreCase to do a a case-insensitve Replace.
		/// </summary>
		/// <param name="original"></param>
		/// <param name="pattern"></param>
		/// <param name="replacement"></param>
		/// <param name="comparisonType"></param>
		/// <param name="stringBuilderInitialSize"></param>
		/// <returns></returns>
		static public string Replace(this string original, string pattern, string replacement, StringComparison comparisonType, int stringBuilderInitialSize)
		{
			if (original == null)
			{
				return null;
			}

			if (String.IsNullOrEmpty(pattern))
			{
				return original;
			}


			int posCurrent = 0;
			int lenPattern = pattern.Length;
			int idxNext = original.IndexOf(pattern, comparisonType);
			StringBuilder result = new StringBuilder(stringBuilderInitialSize < 0 ? Math.Min(4096, original.Length) : stringBuilderInitialSize);

			while (idxNext >= 0)
			{
				result.Append(original, posCurrent, idxNext - posCurrent);
				result.Append(replacement);

				posCurrent = idxNext + lenPattern;

				idxNext = original.IndexOf(pattern, posCurrent, comparisonType);
			}

			result.Append(original, posCurrent, original.Length - posCurrent);

			return result.ToString();
		}
	}
}
