using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleasePlan
{
    [Command("Comment Checklist", CommandGroup.ReleasePlan)]
    public class UpdateCheckListCommentCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public CheckListItem CheckListItem { get; set; }
    }
}
