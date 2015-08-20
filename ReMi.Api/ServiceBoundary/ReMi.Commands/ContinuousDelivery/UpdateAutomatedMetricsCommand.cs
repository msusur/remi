using System;
using ReMi.Common.Constants.ReleaseExecution;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ContinuousDelivery
{
    [Command("Update automated release metrics", CommandGroup.ContinuousDelivery)]
    public class UpdateAutomatedMetricsCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }
        public Guid ReleaseWindowId { get; set; }
        public MetricType MetricType { get; set; }

        public override string ToString()
        {
            return String.Format("[ReleaseWindowId={0}, MetricType={1}, Context={2}]", ReleaseWindowId, MetricType, CommandContext);
        }
    }
}
