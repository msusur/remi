using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleasePlan
{
    [Command("Remove Approver", CommandGroup.ReleasePlan)]
    public class RemoveReleaseApproverCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Guid ApproverId{ get; set; }

        public Guid ReleaseWindowId { get; set; }

        public override string ToString()
        {
            return string.Format("[CommandContext={0}, ApproverId={1}, ReleaseWindowId]", CommandContext, ApproverId,
                ReleaseWindowId);
        }
    }
}
