using ReMi.Contracts.Cqrs.Commands;
using System;

namespace ReMi.Commands.Metrics
{
    [Command("Calculate deploy time", CommandGroup.Metrics, IsBackground = true)]
    public class CalculateDeployTimeCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Guid ReleaseWindowId { get; set; }

        public override string ToString()
        {
            return String.Format("[ReleaseWindowId={0}, CommandContext={1}]",
                ReleaseWindowId, CommandContext);
        }

    }
}
