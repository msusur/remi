using System.Collections.Generic;
using ReMi.BusinessEntities.BusinessRules;
using System;
using ReMi.Common.Constants.BusinessRules;

namespace ReMi.DataAccess.BusinessEntityGateways.BusinessRules
{
    public interface IBusinessRuleGateway : IDisposable
    {
        BusinessRuleDescription GetBusinessRule(Guid ruleId);
        BusinessRuleDescription GetBusinessRule(BusinessRuleGroup group, string rule);
        Guid? GetBusinessRuleId(BusinessRuleGroup group, string rule);
        IEnumerable<BusinessRuleView> GetBusinessRules();

        void CreateBusinessRule(BusinessRuleDescription ruleDescription);
        void DeleteBusinessRule(Guid ruleId);
        void UpdateRuleScript(Guid ruleId, string script);
        void UpdateTestData(Guid testDataId, string testDataJson);
        void UpdateAccountTestData(Guid testDataId, string testDataJson);
        void AddRuleToCommand(Guid ruleId, int commandId);
        void AddRuleToQuery(Guid ruleId, int queryId);
    }
}
