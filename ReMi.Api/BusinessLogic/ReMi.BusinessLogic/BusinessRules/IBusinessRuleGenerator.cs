using System;
using ReMi.BusinessEntities.BusinessRules;

namespace ReMi.BusinessLogic.BusinessRules
{
    public interface IBusinessRuleGenerator
    {
        BusinessRuleDescription GenerateCommandRule(Type commandType, Guid accountId);
        BusinessRuleDescription GenerateQueryRule(Type queryType, Guid accountId);
    }
}
