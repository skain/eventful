using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventfulBackend.EventfulAggregation
{
	public class SingleAggregateResult
	{
		public string GroupName { get; set; }
		public long? AggregateValue { get; set; }

	}
}
