using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.SourceControl
{
    [Command("Reload Release Repositories", CommandGroup.SourceControl)]
    public class LoadReleaseRepositoriesCommand : ICommand
    {
        public Guid ReleaseWindowId { get; set; }

        public CommandContext CommandContext { get; set; }

        public override string ToString()
        {
            return string.Format("ReleaseWindowId={0}, CommandContext={1}", ReleaseWindowId,
                CommandContext);
        }
    }
}
