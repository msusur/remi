using ReMi.Contracts.Cqrs.Commands;
using System;
using System.Collections.Generic;

namespace ReMi.Commands.ReleasePlan
{
    [Command("Change release task order", CommandGroup.ReleaseTask)]
    public class UpdateReleaseTasksOrderCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public IDictionary<Guid, short> ReleaseTasksOrder { get; set; }
    }
}
