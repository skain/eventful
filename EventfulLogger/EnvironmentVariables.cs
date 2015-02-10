using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventfulLogger
{
	public static class EnvironmentVariables
	{
		public static string MachineName
		{
			get
			{
				return Environment.MachineName;
			}
		}

		public static string UserName
		{
			get
			{
				string username = HttpUser;
				
				if (string.IsNullOrEmpty(username))
				{
					username = ApplicationRunningAs;
				}

				return username;
			}
		}

		public static string ApplicationRunningAs
		{
			get
			{
				return Environment.UserName;
			}
		}

		public static string HttpUser
		{
			get
			{
				string user = "";
				try
				{
					user = System.Web.HttpContext.Current.User.Identity.Name;
				}
				catch
				{
					user = "Anon";
				}

				if (string.IsNullOrWhiteSpace(user))
				{
					user = "Anon";
				}

				return user;
			}
		}

		public static string ApplicationName
		{
			get
			{
				string name = System.AppDomain.CurrentDomain.FriendlyName;
				if (name != null)
				{
					name = name.Replace(".exe", "");
				}

				return name;
			}
		}
	}
}
