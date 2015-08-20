using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleasePlan
{
    [Command("Create Task", CommandGroup.ReleaseTask)]
    public class CreateReleaseTaskCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public ReleaseTask ReleaseTask { get; set; }

        public Account Assignee { get; set; }

        public override string ToString()
        {
            return string.Format("[CommandContext = {0}, ReleaseTask = {1}, Assignee={2}]",
                CommandContext, ReleaseTask, Assignee);
        }
    }
}
