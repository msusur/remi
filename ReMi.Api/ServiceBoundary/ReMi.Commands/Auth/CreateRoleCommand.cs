using ReMi.BusinessEntities.Auth;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.Auth
{
    [Command("Create Role", CommandGroup.AccessControl)]
    public class CreateRoleCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Role Role { get; set; }

        public override string ToString()
        {
            return string.Format("[Role = {0}]", Role);
        }
    }
}
