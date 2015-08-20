using ReMi.DataEntities.BusinessRules;
using System;
using System.Collections.Generic;

namespace ReMi.DataAccess.Exceptions
{
    public class BusinessRuleNotNotFoundException : EntityNotFoundException
    {
        public BusinessRuleNotNotFoundException(Guid ruleId) :
            base("BusinessRule", ruleId)
        {
        }

        public BusinessRuleNotNotFoundException(Guid ruleId, Exception innerException) :
            base("BusinessRule", ruleId, innerException)
        {
        }

        public BusinessRuleNotNotFoundException(List<BusinessRuleDescription> rules) :
            base("BusinessRule", rules)
        {
        }

        public BusinessRuleNotNotFoundException(List<BusinessRuleDescription> rules, Exception innerException) :
            base("BusinessRule", rules, innerException)
        {
        }
    }
}
