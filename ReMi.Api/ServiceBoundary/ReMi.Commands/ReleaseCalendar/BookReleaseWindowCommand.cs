using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleaseCalendar
{
    [Command("Schedule Release", CommandGroup.ReleaseCalendar)]
    public class BookReleaseWindowCommand : BasicReleaseWindowCommand
    {
    }
}
