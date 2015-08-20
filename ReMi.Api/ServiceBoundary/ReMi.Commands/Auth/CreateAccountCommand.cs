using ReMi.BusinessEntities.Auth;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.Auth
{
    [Command("Create Account Command", CommandGroup.AccessControl)]
    public class CreateAccountCommand : ICommand
    {
        public Account Account { get; set; }

        public override string ToString()
        {
            return string.Format("[Account = {0}]", Account);
        }

        public CommandContext CommandContext { get; set; }
    }
}
