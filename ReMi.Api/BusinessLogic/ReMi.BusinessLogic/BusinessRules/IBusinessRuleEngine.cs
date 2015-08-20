using System;
using ReMi.BusinessEntities.BusinessRules;
using System.Collections.Generic;
using ReMi.Common.Constants.BusinessRules;

namespace ReMi.BusinessLogic.BusinessRules
{
    public interface IBusinessRuleEngine
    {
        object Execute(Guid userId, BusinessRuleGroup group, string rule, IDictionary<string, string> parameters);
        object Execute(Guid userId, Guid ruleId, IDictionary<string, string> parameters);
        T Execute<T>(Guid userId, Guid ruleId, IDictionary<string, object> parameters);

        Type GetType(string type);
        object Test(BusinessRuleDescription ruleDescription);
    }
}
