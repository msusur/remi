using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleasePlan
{
    [Command("Remove Checklick Question", CommandGroup.ReleasePlan)]
    public class RemoveCheckListQuestionCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Guid CheckListQuestionId { get; set; }
        public bool ForWholeProduct { get; set; }

        public override string ToString()
        {
            return string.Format("[CommandContext={0}, CheckListQuestionId={1}, ForWholeProduct={2}]",
                CommandContext, CheckListQuestionId, ForWholeProduct);
        }
    }
}
