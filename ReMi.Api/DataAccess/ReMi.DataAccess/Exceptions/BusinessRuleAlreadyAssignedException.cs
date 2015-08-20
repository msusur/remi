using System;

namespace ReMi.DataAccess.Exceptions
{
    public class BusinessRuleAlreadyAssignedException : Exception
    {
        public BusinessRuleAlreadyAssignedException(string name, Guid ruleId)
            : base(FormatMessage(name, ruleId))
        {
        }

        public BusinessRuleAlreadyAssignedException(string name, Guid ruleId, Exception innerException)
            : base(FormatMessage(name, ruleId), innerException)
        {
        }

        private static string FormatMessage(string name, Guid ruleId)
        {
            return string.Format("{0} alredy has rule with id '{1}'", name, ruleId);
        }
    }
}
