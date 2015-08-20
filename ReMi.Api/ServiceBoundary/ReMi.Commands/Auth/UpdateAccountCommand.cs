using ReMi.BusinessEntities.Auth;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.Auth
{
    [Command("Update Account", CommandGroup.AccessControl)]
    public class UpdateAccountCommand : ICommand
    {
        public Account Account { get; set; }

        public override string ToString()
        {
            return string.Format("[Account = {0}]", Account);
        }

        public CommandContext CommandContext { get; set; }
    }
}
