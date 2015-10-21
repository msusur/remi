using System;
using ReMi.BusinessEntities.Metrics;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.Metrics
{
    [Command("Update release metrics with time now", CommandGroup.Metrics)]
    public class UpdateMetricsCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }
        public Guid ReleaseWindowId { get; set; }
        public Metric Metric { get; set; }

        public override string ToString()
        {
            return String.Format("[ReleaseWindowId={0}, Metric={1}, Context={2}]", ReleaseWindowId, Metric,
                CommandContext);
        }
    }
}
