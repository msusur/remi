using ReMi.Commands.BusinessRules;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.CommandHandlers.BusinessRules
{
    public class SaveCommandPermissionRuleCommandHandler : SavePermissionRuleCommandHandlerBase, IHandleCommand<SaveCommandPermissionRuleCommand>
    {
        public void Handle(SaveCommandPermissionRuleCommand command)
        {
            using (var gateway = BusinessRuleGatewayFactory())
            {
                if (HandleAndReturnIsNew(command, gateway))
                    gateway.AddRuleToCommand(command.Rule.ExternalId, command.CommandId);
            }
        }
    }
}
