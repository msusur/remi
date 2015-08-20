using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.Acknowledge
{
    [Command("Clean Release Acknowledgment", CommandGroup.AcknowledgeRelease, IsBackground = true)]
    public class ClearReleaseAcknowledgesCommand : ICommand
    {
        public Guid ReleaseWindowId { get; set; }

        public CommandContext CommandContext { get; set; }
    }
}
