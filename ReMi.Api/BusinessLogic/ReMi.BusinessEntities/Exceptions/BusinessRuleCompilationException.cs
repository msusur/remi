using System;
using ReMi.Common.Constants;
using ReMi.Common.Constants.BusinessRules;
using ReMi.Common.Utils.Enums;

namespace ReMi.BusinessEntities.Exceptions
{
    public class BusinessRuleCompilationException : ApplicationException
    {
        public BusinessRuleCompilationException(BusinessRuleGroup group, string rule,
            string script, string compilationError)
            : base(FormatMessage(group, rule, script, compilationError))
        {
        }

        public BusinessRuleCompilationException(BusinessRuleGroup group, string rule,
            string script, string compilationError, Exception innerException)
            : base(FormatMessage(group, rule, script, compilationError), innerException)
        {
        }

        private static string FormatMessage(BusinessRuleGroup group, string rule, string script, string compilationError)
        {
            return string.Format("Rule '{1}' form group '{2}' has script fail to compile:{0}{3}{0}{0}{4}",
                Environment.NewLine,
                rule,
                EnumDescriptionHelper.GetDescription(group),
                script, compilationError);
        }


    }
}
