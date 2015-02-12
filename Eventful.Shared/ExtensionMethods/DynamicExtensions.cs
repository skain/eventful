using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using Newtonsoft.Json.Linq;

namespace Eventful.Shared.ExtensionMethods
{
	public static class DynamicExtensions
	{
		public static ExpandoObject ToDynamic(this object value)
		{
			IDictionary<string, object> expando = new ExpandoObject();

			Type type = value.GetType();
			var properties = TypeDescriptor.GetProperties(type);
			foreach (PropertyDescriptor property in properties)
			{
				object val = property.GetValue(value);
				expando.Add(property.Name, val);
			}

			return expando as ExpandoObject;
		}

		public static string Dump(this ExpandoObject value)
		{
			string output = string.Empty;

			if (value != null)
			{
				foreach (KeyValuePair<string, object> row in (IDictionary<string, object>)value)
					output = output + "Key: " + row.Key + ", Value: " + row.Value.ToString() + Environment.NewLine;
			}

			return output;
		}
	}
}
