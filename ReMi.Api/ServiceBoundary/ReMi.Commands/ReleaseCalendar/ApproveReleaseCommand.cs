using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleaseCalendar
{
    [Command("Approve release", CommandGroup.ReleasePlan)]
    public class ApproveReleaseCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Guid ReleaseWindowId { get; set; }

        public Guid AccountId { get; set; }

        public String Comment { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public override string ToString()
        {
            return string.Format("[ReleaseWindowId={0}, AccountId={1}, CommandContext={2}, Comment={3}, UserName={4}]",
                ReleaseWindowId, AccountId, CommandContext, Comment, UserName);
        }
    }
}
