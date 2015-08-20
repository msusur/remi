using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleaseExecution
{
    [Command("Remove account from sign off release list", CommandGroup.ReleaseExecution)]
    public class RemoveSignOffCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }
        public Guid SignOffId { get; set; }
        public Guid AccountId { get; set; }
        public Guid ReleaseWindowId { get; set; }

        public override string ToString()
        {
            return String.Format("[SignOffId={0}, ReleaseWindowId={1}, Accountid={2}, Context={3}]", SignOffId,
                ReleaseWindowId, AccountId,
                CommandContext);
        }
    }
}
