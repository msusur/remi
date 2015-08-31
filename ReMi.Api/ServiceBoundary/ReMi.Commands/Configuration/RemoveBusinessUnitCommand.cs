using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.Configuration
{
    [Command("Remove existing business unit", CommandGroup.Configuration)]
    public class RemoveBusinessUnitCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Guid ExternalId { get; set; }

        public override string ToString()
        {
            return string.Format("CommandContext={0}, ExternalId={1}",
                CommandContext, ExternalId);
        }
    }
}
