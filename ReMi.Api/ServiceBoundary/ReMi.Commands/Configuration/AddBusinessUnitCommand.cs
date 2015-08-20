using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.Configuration
{
    [Command("Add business unit", CommandGroup.Configuration)]
    public class AddBusinessUnitCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public Guid ExternalId { get; set; }

        public override string ToString()
        {
            return String.Format("CommandContext={0}, Name={1}, Description={2}, ExternalId={3}",
                CommandContext, Name, Description, ExternalId);
        }
    }
}
