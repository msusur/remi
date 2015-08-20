using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.DeploymentTool
{
    [Command("Re-populate deployment time measurements", CommandGroup.DeploymentTool)]
    public class RePopulateDeploymentMeasurementsCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Guid ReleaseWindowId { get; set; }

        public override string ToString()
        {
            return String.Format("[ReleaseWindowId={0}, Context={1}]", 
                ReleaseWindowId, CommandContext);
        }
    }
}
