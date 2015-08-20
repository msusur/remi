using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleasePlan
{
    [Command("Create Help Desk Task", CommandGroup.ReleaseTask, IsBackground = true)]
    public class CreateHelpDeskTaskCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public ReleaseTask ReleaseTask { get; set; }

        public override string ToString()
        {
            return string.Format("[ReleaseTask={0}, CommandContext={1}]",
                ReleaseTask, CommandContext);
        }
    }
}
