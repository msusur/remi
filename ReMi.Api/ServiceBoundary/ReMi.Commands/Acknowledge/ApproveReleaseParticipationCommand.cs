using ReMi.Contracts.Cqrs.Commands;
using System;

namespace ReMi.Commands.Acknowledge
{
    [Command("Acknowledge Release Participation", CommandGroup.AcknowledgeRelease, IsBackground = true)]
    public class ApproveReleaseParticipationCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Guid ReleaseParticipantId { get; set; }

        public override string ToString()
        {
            return String.Format("{0} with ReleaseParticipantId: {1}", GetType().Name, ReleaseParticipantId);
        }
    }
}
