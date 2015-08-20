using ReMi.Commands.BusinessRules;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.CommandHandlers.BusinessRules
{
    public class SaveQueryPermissionRuleCommandHandler : SavePermissionRuleCommandHandlerBase, IHandleCommand<SaveQueryPermissionRuleCommand>
    {
        public void Handle(SaveQueryPermissionRuleCommand command)
        {
            using (var gateway = BusinessRuleGatewayFactory())
            {
                if (HandleAndReturnIsNew(command, gateway))
                    gateway.AddRuleToQuery(command.Rule.ExternalId, command.QueryId);
            }
        }
    }
}
