using System;
using ReMi.BusinessEntities.DeploymentTool;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleasePlan
{
    [Command("Update release job", CommandGroup.ReleasePlan)]
    public class UpdateReleaseJobCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Guid ReleaseWindowId { get; set; }

        public ReleaseJob ReleaseJob { get; set; }

        public override string ToString()
        {
            return string.Format("[ReleaseJob={0}, CommandContext={1}]",
                ReleaseJob, CommandContext);
        }
    }
}
