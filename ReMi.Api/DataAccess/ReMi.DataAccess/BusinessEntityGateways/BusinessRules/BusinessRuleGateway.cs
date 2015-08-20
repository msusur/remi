using System.Text.RegularExpressions;
using AutoMapper;
using ReMi.BusinessEntities.BusinessRules;
using ReMi.Common.Constants.BusinessRules;
using ReMi.DataAccess.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.Common.Utils.Repository;
using DataBusinessRule = ReMi.DataEntities.BusinessRules.BusinessRuleDescription;
using DataBusinessRuleTestData = ReMi.DataEntities.BusinessRules.BusinessRuleTestData;
using DataBusinessRuleAccountTestData = ReMi.DataEntities.BusinessRules.BusinessRuleAccountTestData;
using DataCommand = ReMi.DataEntities.Api.Command;
using DataQuery = ReMi.DataEntities.Api.Query;

namespace ReMi.DataAccess.BusinessEntityGateways.BusinessRules
{
    public class BusinessRuleGateway : BaseGateway, IBusinessRuleGateway
    {
        public IRepository<DataBusinessRule> BusinessRuleRepository { get; set; }
        public IRepository<DataBusinessRuleTestData> BusinessRuleTestDataRepository { get; set; }
        public IRepository<DataBusinessRuleAccountTestData> BusinessRuleAccountTestDataRepository { get; set; }
        public IRepository<DataCommand> CommandRepository { get; set; }
        public IRepository<DataQuery> QueryRepository { get; set; }
        public IMappingEngine MappingEngine { get; set; }

        public override void OnDisposing()
        {
            BusinessRuleRepository.Dispose();
            BusinessRuleTestDataRepository.Dispose();
            BusinessRuleAccountTestDataRepository.Dispose();
            CommandRepository.Dispose();
            QueryRepository.Dispose();

            base.OnDisposing();
        }

        public BusinessRuleDescription GetBusinessRule(Guid ruleId)
        {
            var ruleDescription = BusinessRuleRepository.GetSatisfiedBy(x => x.ExternalId == ruleId);
            return ruleDescription == null ? null
                : MappingEngine.Map<DataBusinessRule, BusinessRuleDescription>(ruleDescription);
        }

        public BusinessRuleDescription GetBusinessRule(BusinessRuleGroup group, string rule)
        {
            var ruleDescription = BusinessRuleRepository.GetSatisfiedBy(x => x.Name == rule && x.Group == group);
            return ruleDescription == null ? null
                : MappingEngine.Map<DataBusinessRule, BusinessRuleDescription>(ruleDescription);
        }

        public Guid? GetBusinessRuleId(BusinessRuleGroup group, string rule)
        {
            var ruleDescription = BusinessRuleRepository.GetSatisfiedBy(x => x.Name == rule && x.Group == group);
            return ruleDescription == null ? (Guid?)null : ruleDescription.ExternalId;
        }

        public IEnumerable<BusinessRuleView> GetBusinessRules()
        {
            return BusinessRuleRepository.Entities
                .OrderBy(x => x.Group.ToString())
                .ThenBy(x => x.Name)
                .ToArray()
                .Select(x => MappingEngine.Map<DataBusinessRule, BusinessRuleView>(x))
                .ToArray();
        }

        public void CreateBusinessRule(BusinessRuleDescription ruleDescription)
        {
            var dataRule = MappingEngine.Map<BusinessRuleDescription, DataBusinessRule>(ruleDescription);
            BusinessRuleRepository.Insert(dataRule);
        }

        public void DeleteBusinessRule(Guid ruleId)
        {
            var dataRule = BusinessRuleRepository.GetSatisfiedBy(x => x.ExternalId == ruleId);
            if (dataRule == null)
                return;

            var command = CommandRepository.GetSatisfiedBy(x => x.RuleId == dataRule.BusinessRuleId);
            if (command != null)
            {
                command.RuleId = null;
                CommandRepository.Update(command);
            }
            var query = QueryRepository.GetSatisfiedBy(x => x.RuleId == dataRule.BusinessRuleId);
            if (query != null)
            {
                query.RuleId = null;
                QueryRepository.Update(query);
            }

            BusinessRuleRepository.Delete(dataRule);
        }

        public void UpdateRuleScript(Guid ruleId, string script)
        {
            var dataRule = BusinessRuleRepository.GetSatisfiedBy(x => x.ExternalId == ruleId);
            if (dataRule == null)
                throw new BusinessRuleNotNotFoundException(ruleId);

            dataRule.Script = script;

            BusinessRuleRepository.Update(dataRule);
        }

        public void UpdateTestData(Guid testDataId, string testDataJson)
        {
            var testData = BusinessRuleTestDataRepository.GetSatisfiedBy(x => x.ExternalId == testDataId);
            if (testData == null)
                throw new BusinessRuleTestDataNotNotFoundException(testDataId);

            testData.JsonData = testDataJson;

            BusinessRuleTestDataRepository.Update(testData);
        }

        public void UpdateAccountTestData(Guid testDataId, string testDataJson)
        {
            var testData = BusinessRuleAccountTestDataRepository.GetSatisfiedBy(x => x.ExternalId == testDataId);
            if (testData == null)
                throw new BusinessRuleTestDataNotNotFoundException(testDataId);

            testData.JsonData = testDataJson;

            BusinessRuleAccountTestDataRepository.Update(testData);
        }

        public void AddRuleToCommand(Guid ruleId, int commandId)
        {
            var command = CommandRepository.GetByPrimaryKey(commandId);
            if (command == null)
                throw new CommandNotFoundException(commandId);
            if (command.Rule != null)
                throw new BusinessRuleAlreadyAssignedException(command.Name, ruleId);

            var rule = BusinessRuleRepository.GetSatisfiedBy(x => x.ExternalId == ruleId);
            if (rule == null)
                throw new BusinessRuleNotNotFoundException(ruleId);

            command.RuleId = rule.BusinessRuleId;

            CommandRepository.Update(command);
        }

        public void AddRuleToQuery(Guid ruleId, int queryId)
        {
            var query = QueryRepository.GetByPrimaryKey(queryId);
            if (query == null)
                throw new QueryNotFoundException(queryId);
            if (query.Rule != null)
                throw new BusinessRuleAlreadyAssignedException(query.Name, ruleId);

            var rule = BusinessRuleRepository.GetSatisfiedBy(x => x.ExternalId == ruleId);
            if (rule == null)
                throw new BusinessRuleNotNotFoundException(ruleId);

            query.RuleId = rule.BusinessRuleId;

            QueryRepository.Update(query);
        }
    }
}
