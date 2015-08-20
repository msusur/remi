using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleasePlan
{
    [Command("Add Approver", CommandGroup.ReleasePlan)]
    public class AddReleaseApproversCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public ReleaseApprover[] Approvers { get; set; }

        public override string ToString()
        {
            return string.Format("[CommandContext={0}, Approvers={1}]", CommandContext, Approvers.FormatElements());
        }
    }
}
