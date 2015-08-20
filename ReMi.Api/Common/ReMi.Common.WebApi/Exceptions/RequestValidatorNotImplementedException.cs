using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReMi.Common.WebApi.Exceptions
{
	public class RequestValidatorNotImplementedException<TRequest> : ApplicationException
	{
		public RequestValidatorNotImplementedException()
			: base(FormatMessage())
		{
		}

        public RequestValidatorNotImplementedException(Exception innerException)
            : base(FormatMessage(), innerException)
        {
        }

		private static string FormatMessage()
		{
			return string.Format("IValidateRequest<{0}> is not implemented", typeof(TRequest));
		}
	}
}
