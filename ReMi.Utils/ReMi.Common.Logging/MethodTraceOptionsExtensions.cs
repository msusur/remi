using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReMi.Common.Logging
{
	public static class MethodTraceOptionsExtensions
	{
		public static bool Contains(this MethodTraceOptions option, MethodTraceOptions flagValue)
		{
			return ((option & flagValue) == flagValue);
		}
	}
}
