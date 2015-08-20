using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.Auth
{
    [Command("Extend session duration", CommandGroup.AccessControl, IsBackground = true)]
    public class ProlongSessionCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }
        public Guid SessionId { get; set; }

        public override string ToString()
        {
            return String.Format("[CommandContext={0}, SessionId={1}]", CommandContext, SessionId);
        }
    }
}
