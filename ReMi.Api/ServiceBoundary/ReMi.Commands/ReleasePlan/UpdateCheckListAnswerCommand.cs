using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleasePlan
{
    [Command("Check Checklist", CommandGroup.ReleasePlan)]
    public class UpdateCheckListAnswerCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public CheckListItem CheckListItem { get; set; }
    }
}
