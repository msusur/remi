using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReMi.Common.WebApi.Exceptions
{
	public class QueryHandlerNotImplementedException<TRequest, TResponse> : ApplicationException
	{
		public QueryHandlerNotImplementedException()
			: base(FormatMessage())
		{
		}

		private static string FormatMessage()
		{
			return string.Format("IHandleQuery<{0}, {1}> is not implemented", typeof(TRequest), typeof(TResponse));
		}
	}
}
