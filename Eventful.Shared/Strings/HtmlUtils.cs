using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NLog;
using Eventful.Shared.ExtensionMethods;

namespace Eventful.Shared.Strings
{
	public static class HtmlUtils
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private static readonly Regex _lessThanEscapeRegex = new Regex("<([\\d\\s])");
		/// <summary>
		/// Characters with special meaning in html/xml that we may not want to treat the same as other html entities.
		/// </summary>
		private static readonly string[] xmlEntities = new[] { "amp", "gt", "lt", "quot", "apos", "rsquo", "lsquo", "nbsp" };


		/// <summary>
		/// Strips out HTML to plain text, while keeping line breaks in the appropriate places.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="addFormatting">True to add some ASCII formatting text in place of some HTML tags (hr, li, etc)</param>
		/// <returns></returns>
		public static string StripHtml(string s, bool addFormatting)
		{
			//no html to strip
			if (string.IsNullOrWhiteSpace(s) || s.IndexOf("<") == -1)
				return s;

			s = cleanUpStringForHtmlAgilityPack(s, true);

			if (s.IndexOf("<") == -1)
				return s;

			//parse html
			HtmlAgilityPack.HtmlDocument hdoc = new HtmlAgilityPack.HtmlDocument();
			hdoc.LoadHtml(s);

			//iterates through nodes and child nodes.
			s = StripHtml(hdoc.DocumentNode, addFormatting);

			//If it started with certain block elements (<p>, etc.) an undesired opening line break might have gotten added.
			if (s.StartsWith("\r"))
				s = s.Substring(1);
			if (s.StartsWith("\n"))
				s = s.Substring(1);


			return s;

		}

		/// <summary>
		/// Runs through anti-xss, fixes things like 3&lt;5 that confuse HtmlAgilityPack.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="isForStripHtml">if true, puts in br's for line breaks and doesn't worry about anti-xss altering css classes</param>
		/// <returns></returns>
		private static string cleanUpStringForHtmlAgilityPack(string s, bool isForStripHtml = false)
		{
			//sanitize against XSS before escaping out or parsing anything.
			s = CleanUpHtml(s, preserveClassNames: !isForStripHtml);



			//escape out less than sign if followed immediately by a number or whitespace.
			//(legit use that sometimes borks Html Agility Pack's parser)
			string sTemp = _lessThanEscapeRegex.Replace(s, "&lt;${1}");

			//Html Agility pack will remove line breaks unless we change them to <br/> first.
			//Only do this if stripping html from final output.
			if (isForStripHtml)
				s = sTemp.Replace("\n", "<br/>");

			return s;
		}

		/// <summary>
		/// Strips out HTML to plain text, while keeping line breaks in the appropriate places.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="addFormatting">True to add some ASCII formatting text in place of some HTML tags (hr, li, etc)</param>
		/// <returns></returns>
		public static string StripHtml(HtmlAgilityPack.HtmlNode node, bool addFormatting)
		{
			//text node, just return text.
			if (node.NodeType == HtmlAgilityPack.HtmlNodeType.Text)
				return node.InnerText;

			//line breaks, no inner text.
			if (node.Name == "br" || (!addFormatting && node.Name == "hr"))
			{
				return "\r\n";
			}
			else if (node.Name == "hr")
			{
				return "\r\n======================================================================\r\n";
			}


			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			//recursively iterate through children.
			if (node.HasChildNodes)
			{
				foreach (HtmlAgilityPack.HtmlNode child in node.ChildNodes)
				{
					sb.Append(StripHtml(child, addFormatting));
				}
			}


			string[] blockelements = { "div", "td", "p", "ul", "ol", "dl", "dd", "dt", "li", "blockquote" };



			if (addFormatting)
			{
				string pre = string.Empty;
				string post = string.Empty;

				if (blockelements.Contains(node.Name))
				{
					post = "\r\n";
				}

				if (node.Name == "p" || node.Name == "ul" || node.Name == "ol" || node.Name == "dl") //line break before and after
				{
					pre = "\r\n";
				}
				else if (node.Name == "blockquote") // (line break) "text" (line break)
				{
					pre = "\r\n\"";
					post = "\"\r\n";
				}
				else if (node.Name == "li") //* text (line break)
				{
					pre = "* ";
				}

				return pre + sb.Append(post);
			}
			else if (blockelements.Contains(node.Name))
			{
				sb.Append("\r\n");
			}

			return sb.ToString();
		}

		/// <summary>
		/// Strip out XSS and clean up malformed HTML
		/// </summary>
		/// <param name="dirtyHtml"></param>
		/// <param name="preserveHtmlEntities">True to keep entities that Anti-XSS mangles, such as &gamma;</param>
		/// <param name="preserveClassNames">Set true to undo Anti-XSS prepending x_ to css classes.</param>
		/// <returns></returns>
		public static string CleanUpHtml(string dirtyHtml, bool preserveHtmlEntities = false, bool preserveClassNames = false)
		{
			if (!preserveHtmlEntities && !preserveClassNames)
				return antiXss_GetSafeHtmlFragment(dirtyHtml);
			else
			{
				if (string.IsNullOrWhiteSpace(dirtyHtml))
					return dirtyHtml;

				string cleanHtml = dirtyHtml;
				if (preserveHtmlEntities)
				{
					//get a list of html entity names for special characters, excluding things like &<>"' that could be xss-related.
					IEnumerable<string> entities = HtmlAgilityPack.HtmlEntity.EntityValue.Keys.Cast<string>().Except(xmlEntities);

					//side note: entities are case-sensitive.

					//escape the entities, run sanitizer, then unescape.

					foreach (string entity in entities)
					{
						cleanHtml = cleanHtml.Replace(string.Format("&{0};", entity), string.Format("___{0}___", entity));
					}

					cleanHtml = antiXss_GetSafeHtmlFragment(cleanHtml);

					foreach (string entity in entities)
					{
						cleanHtml = cleanHtml.Replace(string.Format("___{0}___", entity), string.Format("&{0};", entity));
					}
				}
				else
				{
					cleanHtml = antiXss_GetSafeHtmlFragment(cleanHtml);
				}
				if (preserveClassNames)
				{
					//anti-xss prepends x_ on class names.
					cleanHtml = cleanHtml.Replace("class=\"x_", "class=\"", StringComparison.InvariantCultureIgnoreCase).Replace("class='x_", "class='", StringComparison.InvariantCultureIgnoreCase);
				}

				return cleanHtml;
			}
		}

		/// <summary>
		///  Run GetSafeHtmlFragment and then parse for an additional security vulnerability.
		/// </summary>
		/// <param name="dirtyHtml"></param>
		/// <returns></returns>
		private static string antiXss_GetSafeHtmlFragment(string dirtyHtml)
		{
			try
			{
				// From Anti-XSS Sanitizer source:
				// The method transforms and filters HTML of executable scripts. 
				// A safe list of tags and attributes are used to strip dangerous 
				// scripts from the HTML. HTML is also normalized where tags are 
				// properly closed and attributes are properly formatted.
				string s = Microsoft.Security.Application.Sanitizer.GetSafeHtmlFragment(dirtyHtml);

				//This is a hack to try and neutralize a vulnerability in AntiXSS up to v 4.0
				// http://blog.watchfire.com/wfblog/2012/01/microsoft-anti-xss-library-bypass.html
				// Unfortunately MS's fix as of v 4.2.1 is to be stupid agressive at stripping out css and tag attributes, which defeats the whole point of the library.
				// hack fix here is to re-mangle the syntax a bit so that expression() won't be recognized.  Yeah, it's weak sauce.

				return Regex.Replace(s, ":\\w*expression\\w*\\(", ": eexpresion (");

				//bonus question: wtf is "expression()"?
				// IE thought it would be a good idea to let you use a JS expression to generate a CSS rule.  What could possibly go wrong?
			}
			catch (Exception e)
			{
				logger.Error("Error parsing HTML.\r\n{0}", e);
				return "This document's contents have been flagged by Eventful's security systems as possibly containing malicious code. The contents have therefore been removed. If you feel you are receving this message in error please contact WyzAnt Customer Support by phone or email.";
			}
		}
	}

	
}
