using System.Collections.Generic;
using System;
using ReMi.Common.Constants;
using ReMi.Common.Constants.BusinessRules;
using ReMi.Common.Utils.Enums;

namespace ReMi.BusinessEntities.Exceptions
{
	public class BusinessRuleParamtersMissmatchException : ApplicationException
	{
        public BusinessRuleParamtersMissmatchException(BusinessRuleGroup group, string rule,
            IEnumerable<string> receivedParameters, IEnumerable<string> dbParameters)
            : base(FormatMessage(group, rule, receivedParameters, dbParameters))
		{
		}

        public BusinessRuleParamtersMissmatchException(BusinessRuleGroup group, string rule,
            IEnumerable<string> receivedParameters, IEnumerable<string> dbParameters, Exception innerException)
			: base(FormatMessage(group, rule, receivedParameters, dbParameters), innerException)
		{
		}

        private static string FormatMessage(BusinessRuleGroup group, string rule,
            IEnumerable<string> receivedParameters, IEnumerable<string> dbParameters)
		{
			return string.Format("Rule '{1}' form group '{2}' has missmatch parameters:{0}Received parameters: [{3}]{0}Database parameters: [{4}]",
                Environment.NewLine,
                rule,
                EnumDescriptionHelper.GetDescription(group),
                string.Join("; ", receivedParameters),
                string.Join("; ", dbParameters));
		}

		
	}
}
