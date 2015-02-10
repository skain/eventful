using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using eventfulBackend.Utils;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NLog;

namespace eventfulBackend.QueryParsing
{
	public static class eventfulQueryParser
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private static readonly Regex searchStringParserRE = new Regex(@"(?<criteria>(?<conjunction>\|\||&&)*\s*(?<term>\w+)\s*(?<operator>exists|\!exists))|(?<criteria>(?<conjunction>\|\||&&)*\s*(?<term>\w+)\s*(?<operator>==|>=|<=|!=|<|>|in|\!in|matches|\!matches)\s*(?<value>\(.*?\)|'.*?'|\S+|.{0}))", RegexOptions.IgnoreCase);
		//private static readonly Regex searchStringParserRE = new Regex(@"(?<criteria>(?<conjunction>\|\||&&)*\s*(?<term>\w+)\s*(?<operator>==|>=|<=|!=|<|>|in|\!in|matches|\!matches|exists|\!exists)\s*(?<value>\(.*?\)|'.*?'|\S+|.{0}))", RegexOptions.IgnoreCase);
		private static readonly Regex requestedTimeParserRE = new Regex(@"now\s*(?<operator>-|\+)\s*(?<number>\d*)\s*(?<units>second|min|minute|hour|day|week|month)s*", RegexOptions.IgnoreCase);
		private static readonly TimeZoneInfo centralStandardTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");

		public static IMongoQuery ParseSearchStringToIMongoQuery(string toParse)
		{
			//logger.Debug("Query to parse:\r\n{0}", toParse);
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

				//logger.Debug("Criteria: {0} {1} {2}", term, op, val);

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

			//logger.Debug("Mongo Query:\r\n{0}", query.ToString());
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
			double valAsDouble = -1;
			bool valIsNumber = double.TryParse(val, out valAsDouble);
			switch (op.ToLower())
			{
				case "==":
					if (valIsNumber)
					{
						query = Query.EQ(term, valAsDouble);
					}
					else
					{
						query = Query.EQ(term, val);
					}
					break;
				case "!=":
					if (valIsNumber)
					{
						query = Query.NE(term, valAsDouble);
					}
					else
					{
						query = Query.NE(term, val);
					}
					break;
				case ">=":
					if (valIsNumber)
					{
						query = Query.GTE(term, valAsDouble);
					}
					else
					{
						query = Query.GTE(term, val);
					}
					break;
				case "<=":

					if (valIsNumber)
					{
						query = Query.LTE(term, valAsDouble);
					}
					else
					{
						query = Query.LTE(term, val);
					}
					break;
				case "<":
					if (valIsNumber)
					{
						query = Query.LT(term, valAsDouble);
					}
					else
					{
						query = Query.LT(term, val);
					}
					break;
				case ">":
					if (valIsNumber)
					{
						query = Query.GT(term, valAsDouble);
					}
					else
					{
						query = Query.GT(term, val);
					}
					break;
				case "in":
				case "!in":
					if (!(val.StartsWith("(") && val.EndsWith(")")))
					{
						throw new ApplicationException(string.Format("Error. 'in' lists must start and end with parenthesis. Your input: {0}", val));
					}
					val = val.Trim();
					val = val.Substring(1, val.Length - 2); //remove parens
					IEnumerable<string> vals = val.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s =>
					{
						s = s.Trim();
						bool startsWithQuote = s.StartsWith("'");
						bool endsWithQuote = s.EndsWith("'");

						if (!(startsWithQuote || endsWithQuote))
						{
							return s;
						}

						if (startsWithQuote && !endsWithQuote)
						{
							throw new ApplicationException(string.Format("Error. Value in 'in' list starts with a quote but doesn't end with one. Your input: {0}", s));
						}

						if (endsWithQuote && !startsWithQuote)
						{
							throw new ApplicationException(string.Format("Error. Value in 'in' list ends with a quote but doesn't start with one. Your input: {0}", s));
						}

						return s.Substring(1, s.Length - 2); //strip out the quotes from the values
					});
					BsonValue[] bsonVals = vals.Select(s =>
					{
						double d = -1;
						if (double.TryParse(s, out d))
						{
							return (BsonValue)(new BsonDouble(double.Parse(s)));
						}
						else
						{
							return (BsonValue)(new BsonString(s));
						}
					}).ToArray();

					if (op.ToLower() == "!in")
					{
						query = Query.NotIn(term, bsonVals);
					}
					else
					{
						query = Query.In(term, bsonVals);
					}
					break;
				case "matches":
				case "!matches":
					val = removeDelimiters(val, "/");
					query = Query.Matches(term, new BsonRegularExpression(new Regex(val)));
					if (op.ToLower() == "!matches")
					{
						query = Query.Not(query);
					}
					break;
				case "!exists":
					query = Query.NotExists(term);
					break;
				case "exists":
					query = Query.Exists(term);
					break;
				default:
					throw new ApplicationException(string.Format("Error. Unrecognized query operator: {0}", op));
			}
			return query;
		}
		public static DateTime ParseRequestedTime(string time, TimeZoneInfo srcTimeZone)
		{
			var now = DateTime.Now;
			if (time.Trim().ToLower() == "now")
			{
				return now;
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
						retVal = now.AddSeconds(number);
						break;
					case "minute":
					case "min":
						retVal = now.AddMinutes(number);
						break;
					case "hour":
						retVal = now.AddHours(number);
						break;
					case "day":
						retVal = now.AddDays(number);
						break;
					case "week":
						retVal = now.AddDays(number * 7);
						break;
					case "month":
						retVal = now.AddMonths(number);
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
				else
				{
					retVal = TimeZoneUtils.ToSystemLocalTime(retVal, srcTimeZone);
				}
			}

			return retVal;
		}

	}
}
