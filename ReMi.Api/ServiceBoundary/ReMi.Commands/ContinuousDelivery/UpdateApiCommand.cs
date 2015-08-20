using System.Collections.Generic;
using ReMi.BusinessEntities.Api;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ContinuousDelivery
{
    [Command("Updates API information after deploy", CommandGroup.ContinuousDelivery)]
    public class UpdateApiCommand : ICommand
    {
        public List<ApiDescription> ApiDescriptions { get; set; }
        public CommandContext CommandContext { get; set; }
    }
}
