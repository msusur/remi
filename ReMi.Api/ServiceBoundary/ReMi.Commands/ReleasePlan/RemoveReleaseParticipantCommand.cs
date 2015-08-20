using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleasePlan
{
    [Command("Remove Participants", CommandGroup.ReleasePlan)]
    public class RemoveReleaseParticipantCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }
        public ReleaseParticipant Participant { get; set; }
    }
}
