using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleaseCalendar
{
    [Command("Mark release as Failed", CommandGroup.ReleaseCalendar)]
    public class FailReleaseCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Guid ReleaseWindowId { get; set; }
        public String Issues { get; set; }

        public string UserName { get; set; }
        public string Password { get; set; }

        public override string ToString()
        {
            return String.Format("[ReleaseWindowId={0}, Issues={1}, UserName={2}, CommandContext={3}]",
                ReleaseWindowId, Issues, UserName, CommandContext);
        }
    }
}
