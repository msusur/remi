using System;
using ReMi.Commands.BusinessRules;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.BusinessRules;

namespace ReMi.CommandHandlers.BusinessRules
{
    public class DeletePermissionRuleCommandHandler : IHandleCommand<DeletePermissionRuleCommand>
    {
        public Func<IBusinessRuleGateway> BusinessRuleGatewayFacotory { get; set; }
        
        public void Handle(DeletePermissionRuleCommand command)
        {
            using (var gateway = BusinessRuleGatewayFacotory())
            {
                gateway.DeleteBusinessRule(command.RuleId);
            }
        }
    }
}
