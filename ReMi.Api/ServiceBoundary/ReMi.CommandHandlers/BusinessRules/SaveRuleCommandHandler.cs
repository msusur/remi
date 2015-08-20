using System;
using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessLogic.BusinessRules;
using ReMi.Commands.BusinessRules;
using ReMi.Common.Constants.BusinessRules;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.BusinessRules;

namespace ReMi.CommandHandlers.BusinessRules
{
    public class SaveRuleCommandHandler : IHandleCommand<SaveRuleCommand>
    {
        public Func<IBusinessRuleGateway> BusinessRuleGatewayFactory { get; set; }
        public IBusinessRuleEngine BusinessRuleEngine { get; set; }

        public void Handle(SaveRuleCommand command)
        {
            try
            {
                // if result is not bool it will also throw compilation exception.
                BusinessRuleEngine.Test(command.Rule);
            }
            catch (Exception ex)
            {
                throw new BusinessRuleCompilationException(
                    BusinessRuleGroup.Permissions, command.Rule.Name, command.Rule.Script, ex.Message);
            }
            using (var gateway = BusinessRuleGatewayFactory())
            {
                gateway.UpdateRuleScript(command.Rule.ExternalId, command.Rule.Script);
                gateway.UpdateAccountTestData(command.Rule.AccountTestData.ExternalId,
                    command.Rule.AccountTestData.JsonData);
                foreach (var parameter in command.Rule.Parameters)
                {
                    gateway.UpdateTestData(parameter.TestData.ExternalId, parameter.TestData.JsonData);
                }
            }
        }
    }
}
