using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.Configuration
{
    [Command("Remove command from role", CommandGroup.AccessControl)]
    public class RemoveCommandFromRoleCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public int CommandId { get; set; }

        public Guid RoleExternalId { get; set; }
    }
}
