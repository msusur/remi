using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReMi.Common.WebApi.Exceptions
{
	public class CommandHandlerNotImplementedException<TRequest> : ApplicationException
	{
		public CommandHandlerNotImplementedException()
			: base(FormatMessage())
		{
		}

		private static string FormatMessage()
		{
			return string.Format("IHandleCommand<{0}>is not implemented", typeof(TRequest));
		}
	}
}
