using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.BusinessRules;
using ReMi.BusinessLogic.Api;
using ReMi.Common.Constants.BusinessRules;
using ReMi.Common.Utils;
using ReMi.DataAccess.BusinessEntityGateways.Auth;

namespace ReMi.BusinessLogic.BusinessRules
{
    public class BusinessRuleGenerator : IBusinessRuleGenerator
    {  
        public IApiDescriptionBuilder ApiDescriptionBuilder { get; set; }
        public ISerialization Serialization { get; set; }
        public Func<IAccountsGateway> AccountGatewayFactory { get; set; }

        public BusinessRuleDescription GenerateCommandRule(Type commandType, Guid accountId)
        {
            var account = GetAccount(accountId);
            return GenerateRule(commandType, account, "command");
        }

        public BusinessRuleDescription GenerateQueryRule(Type queryType, Guid accountId)
        {
            var account = GetAccount(accountId);
            return GenerateRule(queryType, account, "query");
        }

        private Account GetAccount(Guid accountId)
        {
            if (accountId == Guid.Empty) return null;
            using (var gateway = AccountGatewayFactory())
            {
                return gateway.GetAccount(accountId, true);
            }
        }

        private BusinessRuleDescription GenerateRule(Type type, Account account, string name)
        {
            return new BusinessRuleDescription
            {
                ExternalId = Guid.NewGuid(),
                Description = string.Format("Rule for {1} {0}", type.Name, name),
                Name = string.Format("{0}Rule", type.Name),
                Group = BusinessRuleGroup.Permissions,
                Script = string.Empty,
                AccountTestData = new BusinessRuleAccountTestData
                {
                    ExternalId = Guid.NewGuid(),
                    JsonData = Serialization.ToJson(account)
                },
                Parameters = new List<BusinessRuleParameter>
                {
                    new BusinessRuleParameter
                    {
                        ExternalId = Guid.NewGuid(),
                        Name = name,
                        Type = type.FullName,
                        TestData = new BusinessRuleTestData
                        {
                            ExternalId = Guid.NewGuid(),
                            JsonData = ApiDescriptionBuilder.FormatType(type, 0, null)
                        }
                    }
                }
            };
        }
    }
}
