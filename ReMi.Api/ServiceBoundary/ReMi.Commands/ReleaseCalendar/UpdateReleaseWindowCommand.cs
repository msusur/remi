using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleaseCalendar
{
    [Command("Reschedule Release", CommandGroup.ReleaseCalendar)]
    public class UpdateReleaseWindowCommand : BasicReleaseWindowCommand
    {
        public bool AllowUpdateInPast { get; set; }
    }
}
