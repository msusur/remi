using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.Configuration
{
    [Command("Remove query from role", CommandGroup.AccessControl)]
    public class RemoveQueryFromRoleCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public int QueryId { get; set; }

        public Guid RoleExternalId { get; set; }
    }
}
