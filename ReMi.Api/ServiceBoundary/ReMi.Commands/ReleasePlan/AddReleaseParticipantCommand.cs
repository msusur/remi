using System.Collections.Generic;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleasePlan
{
    [Command("Add Participant", CommandGroup.ReleasePlan)]
    public class AddReleaseParticipantCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }
        public List<ReleaseParticipant> Participants { get; set; }
    }
}
