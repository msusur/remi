using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleasePlan
{
    [Command("Update Task", CommandGroup.ReleaseTask)]
    public class UpdateReleaseTaskCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public ReleaseTask ReleaseTask { get; set; }

        public override string ToString()
        {
            return string.Format("[CommandContext = {0}, ReleaseTask = {1}]",
                CommandContext, ReleaseTask);
        }
    }
}
