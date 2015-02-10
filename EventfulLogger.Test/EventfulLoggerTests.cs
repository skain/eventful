using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace EventfulLogger.Test
{
	[TestFixture]
	public class EventfulLoggerTests
	{
		[Test]
		public void BuildDynamic_EventInfo_Null()
		{
			string eventfulGroup = "TestGroup";
			string message = "Test Message";
			dynamic dyn = EloggerUtils.BuildDynamic(eventfulGroup, null, message, null, null);

			Assert.AreEqual(eventfulGroup, dyn.EventfulGroup);
			Assert.AreEqual(message, dyn.Message);
		}

		[Test]
		public void BuildDynamic_EventInfo_NotNull_Dynamic()
		{
			string eventfulGroup = "TestGroup";
			string message = "Test Message";
			dynamic payload = new { Test = "Test" };
			dynamic dyn = EloggerUtils.BuildDynamic(eventfulGroup, null, message, payload, null);

			Assert.AreEqual(eventfulGroup, dyn.EventfulGroup);
			Assert.AreEqual(message, dyn.Message);
			Assert.AreEqual(payload.Test, dyn.Test);
		}

		[Test]
		public void BuildDynamic_EventInfo_NotNull_Concrete()
		{
			string eventfulGroup = "TestGroup";
			string message = "Test Message";
			string item1 = "TestKey";
			string item2 = "TestVal";
			Tuple<string, string> payload = new Tuple<string, string>(item1, item2);
			dynamic dyn = EloggerUtils.BuildDynamic(eventfulGroup, null, message, payload, null);

			Assert.AreEqual(eventfulGroup, dyn.EventfulGroup);
			Assert.AreEqual(message, dyn.Message);
			Assert.AreEqual(item1, dyn.Item1);
			Assert.AreEqual(item2, dyn.Item2);
		}

		[Test]
		public void BuildDynamic_EventInfo_NotNull_ObjectRef()
		{
			string eventfulGroup = "TestGroup";
			string message = "Test Message";
			string item1 = "TestKey";
			string item2 = "TestVal";
			Tuple<string, string> payload = new Tuple<string, string>(item1, item2);
			object payloadObj = payload;
			dynamic dyn = EloggerUtils.BuildDynamic(eventfulGroup, null, message, payloadObj, null);

			Assert.AreEqual(eventfulGroup, dyn.EventfulGroup);
			Assert.AreEqual(message, dyn.Message);
			Assert.AreEqual(item1, dyn.Item1);
			Assert.AreEqual(item2, dyn.Item2);
		}

		[Test]
		public void BuildDynamic_EventInfo_NotNull_ExpandoObject()
		{
			string eventfulGroup = "TestGroup";
			string message = "Test Message";
			ExpandoObject payload = new ExpandoObject();
			string item1 = "TestKey";
			string item2 = "TestVal";
			((IDictionary<string, object>)payload)[item1] = item2;
			dynamic dyn = EloggerUtils.BuildDynamic(eventfulGroup, null, message, payload, null);

			Assert.AreEqual(eventfulGroup, dyn.EventfulGroup);
			Assert.AreEqual(message, dyn.Message);
			Assert.AreEqual(item2, dyn.TestKey);
		}
	}
}
