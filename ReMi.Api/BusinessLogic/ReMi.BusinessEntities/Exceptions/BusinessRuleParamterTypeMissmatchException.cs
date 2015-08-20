using System;
using ReMi.Common.Constants;
using ReMi.Common.Constants.BusinessRules;
using ReMi.Common.Utils.Enums;

namespace ReMi.BusinessEntities.Exceptions
{
	public class BusinessRuleParamterTypeMissmatchException : ApplicationException
	{
        public BusinessRuleParamterTypeMissmatchException(BusinessRuleGroup group, string rule,
            string parameter, string expectedType, string receivedType)
            : base(FormatMessage(group, rule, parameter, expectedType, receivedType))
		{
		}

        public BusinessRuleParamterTypeMissmatchException(BusinessRuleGroup group, string rule,
            string parameter, string expectedType, string receivedType, Exception innerException)
			: base(FormatMessage(group, rule, parameter, expectedType, receivedType), innerException)
		{
		}

        private static string FormatMessage(BusinessRuleGroup group, string rule,
            string parameter, string expectedType, string receivedType)
		{
			return string.Format("Rule '{1}' form group '{2}' has missmatch parameter [{3}] type:{0}Received parameter type: [{4}]{0}Expected parameter type: [{5}]",
                Environment.NewLine,
                rule,
                EnumDescriptionHelper.GetDescription(group),
                parameter, receivedType, expectedType);
		}

		
	}
}
