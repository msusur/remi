using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessLogic.BusinessRules;
using ReMi.Commands.BusinessRules;
using ReMi.Common.Constants.BusinessRules;
using ReMi.DataAccess.BusinessEntityGateways.BusinessRules;
using System;

namespace ReMi.CommandHandlers.BusinessRules
{
    public abstract class SavePermissionRuleCommandHandlerBase
    {
        public Func<IBusinessRuleGateway> BusinessRuleGatewayFactory { get; set; }
        public IBusinessRuleEngine BusinessRuleEngine { get; set; }

        protected bool HandleAndReturnIsNew(SavePermissionRuleCommandBase command, IBusinessRuleGateway gateway)
        {
            try
            {
                // if result is not bool it will also throw compilation exception.
                var result = (bool)BusinessRuleEngine.Test(command.Rule);
            }
            catch (Exception ex)
            {
                throw new BusinessRuleCompilationException(
                    BusinessRuleGroup.Permissions, command.Rule.Name, command.Rule.Script, ex.Message);
            }
            var rule = gateway.GetBusinessRule(command.Rule.ExternalId);
            if (rule == null)
            {
                gateway.CreateBusinessRule(command.Rule);
                return true;
            }
            gateway.UpdateRuleScript(command.Rule.ExternalId, command.Rule.Script);
            gateway.UpdateAccountTestData(command.Rule.AccountTestData.ExternalId, command.Rule.AccountTestData.JsonData);
            foreach (var parameter in command.Rule.Parameters)
            {
                gateway.UpdateTestData(parameter.TestData.ExternalId, parameter.TestData.JsonData);
            }
            return false;
        }
    }
}
