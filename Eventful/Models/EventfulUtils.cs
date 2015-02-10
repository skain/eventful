using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace eventful.Models
{
	public static class eventfulUtils
	{
		private static readonly Regex searchStringParserRE = new Regex(@"(?<criteria>(?<conjunction>\|\||&&)*\s*(?<term>\w+)\s*(?<operator>==|>=|<=|!=|<|>|in|matches)\s*(?<value>('.*?'|\S*)))", RegexOptions.IgnoreCase);
		private static readonly Regex requestedTimeParserRE = new Regex(@"now\s*(?<operator>-|\+)\s*(?<number>\d*)\s*(?<units>second|minute|hour|day|week|month)s*", RegexOptions.IgnoreCase);

		public static IMongoQuery ParseSearchStringToIMongoQueries(string toParse)
		{
			if (toParse == null)
			{
				return null;
			}
			MatchCollection mc = searchStringParserRE.Matches(toParse);
			IMongoQuery query = null;
			foreach (Match m in mc)
			{
				string conjunction = m.Groups["conjunction"].Value.ToLower();
				string op = m.Groups["operator"].Value;
				string term = m.Groups["term"].Value;
				string val = m.Groups["value"].Value;
				val = removeDelimiters(val, "'");

				IMongoQuery currentCriteriaQuery = buildIMongoQueryFromStringValues(op, term, val);
				if (query == null)
				{
					//this is the first criteria so start the query here
					if (string.IsNullOrEmpty(conjunction))
					{
						query = currentCriteriaQuery;
					}
					else
					{
						throw new ApplicationException(string.Format("Error. Query Criteria cannot begin with conjunction: {0}", conjunction));
					}
				}
				else
				{
					//not the first criteria so add to the existing query
					switch (conjunction)
					{
						case "&&":
							query = Query.And(query, currentCriteriaQuery);
							break;
						case "||":
							query = Query.Or(query, currentCriteriaQuery);
							break;
					}
				}
			}

			return query;
		}

		/// <summary>
		/// Remove the specified delimiter from the beginning and the end of the val string if the val string starts with and ends with the delimiter.
		/// </summary>
		/// <param name="val"></param>
		/// <param name="delimiter"></param>
		/// <returns></returns>
		private static string removeDelimiters(string val, string delimiter)
		{
			val = val.Trim();
			if (val.StartsWith(delimiter) && val.EndsWith(delimiter))
			{
				val = val.Substring(1, val.Length - 2);
			}
			return val;
		}

		private static IMongoQuery buildIMongoQueryFromStringValues(string op, string term, string val)
		{
			IMongoQuery query = null;
			switch (op.ToLower())
			{
				case "==":
					query = Query.EQ(term, val);
					break;
				case "!=":
					query = Query.NE(term, val);
					break;
				case ">=":
					query = Query.GTE(term, val);
					break;
				case "<=":
					query = Query.LTE(term, val);
					break;
				case "<":
					query = Query.LT(term, val);
					break;
				case ">":
					query = Query.GT(term, val);
					break;
				case "in":
					if (!(val.StartsWith("(") && val.EndsWith(")")))
					{
						throw new ApplicationException(string.Format("Error. 'in' lists must start and end with parenthesis. Your input: {0}", val));
					}
					val = val.Trim();
					val = val.Substring(1, val.Length - 2);
					BsonValue[] vals = val.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => new BsonString(s.Trim())).ToArray();
					query = Query.In(term, vals);
					break;
				case "matches":
					val = removeDelimiters(val, "/");
					query = Query.Matches(term, new BsonRegularExpression(new Regex(val)));
					break;
				default:
					throw new ApplicationException(string.Format("Error. Unrecognized query operator: {0}", op));
			}
			return query;
		}

		public static DateTime ParseRequestedTime(string time)
		{
			if (time.Trim().ToLower() == "now")
			{
				return DateTime.Now;
			}

			MatchCollection mc = requestedTimeParserRE.Matches(time);
			DateTime retVal = new DateTime();

			if (mc.Count > 0)
			{
				Match m = mc[0];
				string numberStr = m.Groups["number"].Value;
				int number = 0;
				if (!int.TryParse(numberStr, out number))
				{
					throw new ApplicationException(string.Format("Error. Number of units supplied in DateTime string is invalid. Units: {0}, Full DateTime: {1}", numberStr, time));
				}

				string op = m.Groups["operator"].Value;
				if (op == "-")
				{
					number = number * -1;
				}


				string units = m.Groups["units"].Value;

				switch (units)
				{
					case "second":
						retVal = DateTime.Now.AddSeconds(number);
						break;
					case "minute":
						retVal = DateTime.Now.AddMinutes(number);
						break;
					case "hour":
						retVal = DateTime.Now.AddHours(number);
						break;
					case "day":
						retVal = DateTime.Now.AddDays(number);
						break;
					case "week":
						retVal = DateTime.Now.AddDays(number * 7);
						break;
					case "month":
						retVal = DateTime.Now.AddMonths(number);
						break;
					default:
						throw new ApplicationException(string.Format("Error. Unrecognized unit in DateTime string: {0}", units));
				}
			}
			else
			{
				if (!DateTime.TryParse(time, out retVal))
				{
					throw new ApplicationException(string.Format("Error. Times must be either a parseable date or a calculation based off of now. You entered: {0}", time));
				}
			}

			return retVal;
		}
	}
}