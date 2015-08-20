using System;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Plugins.Data.SourceControl;

namespace ReMi.Commands.SourceControl
{
    [Command("Update release repository informations", CommandGroup.SourceControl)]
    public class UpdateReleaseRepositoryCommand : ICommand
    {
        public ReleaseRepository Repository { get; set; }
        public Guid ReleaseWindowId { get; set; }

        public CommandContext CommandContext { get; set; }

        public override string ToString()
        {
            return string.Format("[Repository={0}, ReleaseWindowsId={1}, CommandContext={2}]",
                Repository, ReleaseWindowId, CommandContext);
        }
    }
}
