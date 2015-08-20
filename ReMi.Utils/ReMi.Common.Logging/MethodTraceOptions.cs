using System;

namespace ReMi.Common.Logging
{
	[Flags]
	public enum MethodTraceOptions
	{
		MinimalVerbose = 0,
		IncludeArgumentNames = 1 << 0,
        TraceExceptionsThrown = 1 << 1,
        TurnOff = 1 << 2,
	}
}
