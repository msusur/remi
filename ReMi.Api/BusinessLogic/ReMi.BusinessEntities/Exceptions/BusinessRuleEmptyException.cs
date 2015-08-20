using System;
using ReMi.Common.Constants;

namespace ReMi.BusinessEntities.Exceptions
{
	public class BusinessRuleEmptyException : ApplicationException
	{
        public BusinessRuleEmptyException()
            : base(FormatMessage())
		{
		}

        public BusinessRuleEmptyException(Exception innerException)
			: base(FormatMessage(), innerException)
		{
		}

        private static string FormatMessage()
		{
			return string.Format("Rule not exists or has empty script");
		}

		
	}
}
