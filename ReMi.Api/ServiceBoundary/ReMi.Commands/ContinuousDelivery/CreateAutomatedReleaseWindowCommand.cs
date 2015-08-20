using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ContinuousDelivery
{
    [Command("Create Automated Release Window", CommandGroup.ContinuousDelivery)]
    public class CreateAutomatedReleaseWindowCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Guid ExternalId { get; set; }
        public string Product { get; set; }
        public string Description { get; set; }
    }
}
