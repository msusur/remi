using System;

namespace ReMi.Common.WebApi.Exceptions
{
	public class TrackerNotImplementedException<T> : ApplicationException
	{
        public TrackerNotImplementedException()
			: base(FormatMessage(typeof(T)))
		{
		}

		private static string FormatMessage(Type type)
		{
            return string.Format("{0} is not implemented", type.Name);
		}
	}
}
