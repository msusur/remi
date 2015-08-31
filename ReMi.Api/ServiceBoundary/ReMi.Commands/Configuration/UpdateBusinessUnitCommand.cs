using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.Configuration
{
    [Command("Update existing business unit", CommandGroup.Configuration)]
    public class UpdateBusinessUnitCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public Guid ExternalId { get; set; }

        public override string ToString()
        {
            return string.Format("CommandContext={0}, Name={1}, Description={2}, ExternalId={3}",
                CommandContext, Name, Description, ExternalId);
        }
    }
}
