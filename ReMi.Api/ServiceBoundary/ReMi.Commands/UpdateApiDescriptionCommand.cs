using System;
using ReMi.BusinessEntities.Api;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands
{
    [Command("Updates API description", CommandGroup.Api)]
    public class UpdateApiDescriptionCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }
        public ApiDescription ApiDescription { get; set; }

        public override string ToString()
        {
            return String.Format("[CommandContext={0}, ApiDescription={1}]", CommandContext, ApiDescription);
        }
    }
}
