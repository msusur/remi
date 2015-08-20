using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.Auth;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleaseCalendar
{
    [Command("Close Release", CommandGroup.ReleaseCalendar)]
    public class CloseReleaseCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public List<Account> Recipients { get; set; }
        public String ReleaseNotes { get; set; }
        public Guid ReleaseWindowId { get; set; }

        public string UserName { get; set; }
        public string Password { get; set; }

        public override string ToString()
        {
            return String.Format("[Recipients={0}, ReleaseNotes={1}, ReleaseWindowId={2}, UserName={3}, CommandContext={4}]",
                Recipients.FormatElements(), ReleaseNotes, ReleaseWindowId, UserName, CommandContext);
        }
    }
}
