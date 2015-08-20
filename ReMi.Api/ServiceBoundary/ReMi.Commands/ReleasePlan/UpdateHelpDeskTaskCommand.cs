using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleasePlan
{
    [Command("Update Help Desk Task", CommandGroup.ReleaseTask, IsBackground = true)]
    public class UpdateHelpDeskTaskCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public ReleaseTask ReleaseTask { get; set; }
    }
} ;
