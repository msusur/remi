using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleasePlan
{
    [Command("Add Checklist question", CommandGroup.ReleasePlan)]
    public class AddCheckListQuestionsCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public IEnumerable<CheckListQuestion> QuestionsToAdd { get; set; }
        public IEnumerable<CheckListQuestion> QuestionsToAssign { get; set; } 

        public Guid ReleaseWindowId { get; set; }

        public override string ToString()
        {
            return String.Format("[QuestionsToAdd={0}, QuestionsToAssign={1}, ReleaseWindowId={2}, CommandContext={3}]",
                QuestionsToAdd.FormatElements(), QuestionsToAssign.FormatElements(), ReleaseWindowId, CommandContext);
        }
    }
}
