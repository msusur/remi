using ReMi.Contracts.Cqrs.Commands;
using System;
using ReMi.Common.Constants.ReleaseExecution;

namespace ReMi.Commands.Metrics
{
    [Command("Update release metrics with specific datetime", CommandGroup.Metrics)]
    public class UpdateMetricsWithDateTimeCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }
        public Guid ReleaseWindowId { get; set; }
        public MetricType MetricType { get; set; }
        public DateTime ExecutedOn { get; set; }

        public override string ToString()
        {
            return String.Format("[ReleaseWindowId={0}, MetricType={1}, ExecutedOn={2}, Context={3}]",
                ReleaseWindowId, MetricType, ExecutedOn, CommandContext);
        }
    }
}
