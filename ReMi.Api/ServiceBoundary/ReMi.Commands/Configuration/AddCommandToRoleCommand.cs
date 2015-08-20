using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.Configuration
{
    [Command("Add command to role", CommandGroup.AccessControl)]
    public class AddCommandToRoleCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public int CommandId { get; set; }

        public Guid RoleExternalId { get; set; }
    }
}
