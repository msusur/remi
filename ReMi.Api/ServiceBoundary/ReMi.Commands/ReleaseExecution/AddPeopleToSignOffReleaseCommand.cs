using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.ReleaseExecution;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleaseExecution
{
    [Command("Add people to sign off release", CommandGroup.ReleaseExecution)]
    public class AddPeopleToSignOffReleaseCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public List<SignOff> SignOffs { get; set; }
        public Guid ReleaseWindowId { get; set; }
        public Boolean IsBackground { get; set; }

        public override string ToString()
        {
            return String.Format("[SignOffs={0}, ReleaseWindowId={1}, Context={2}]", SignOffs.FormatElements(),
                ReleaseWindowId, CommandContext);
        }
    }
}
