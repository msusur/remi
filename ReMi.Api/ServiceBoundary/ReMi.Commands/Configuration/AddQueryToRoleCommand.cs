using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.Configuration
{
    [Command("Add query to role", CommandGroup.AccessControl)]
    public class AddQueryToRoleCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public int QueryId { get; set; }

        public Guid RoleExternalId { get; set; }
    }
}
