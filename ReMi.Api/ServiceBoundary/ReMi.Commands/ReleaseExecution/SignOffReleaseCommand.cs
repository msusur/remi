using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleaseExecution
{
    [Command("Sign off release", CommandGroup.ReleaseExecution)]
    public class SignOffReleaseCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }
        public Guid AccountId { get; set; }
        public Guid ReleaseWindowId { get; set; }
        public String Comment { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public override string ToString()
        {
            return String.Format("[AccountId={0}, ReleaseWindowId={1}, Context={2}, Comment={3}, UserName={4}]", AccountId,
                ReleaseWindowId,
                CommandContext, Comment, UserName);
        }
    }
}
