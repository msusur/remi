using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.SourceControl
{
    [Command("Store released changes", CommandGroup.SourceControl, IsBackground = true)]
    public class StoreSourceControlChangesCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }
        public Guid ReleaseWindowId { get; set; }

        public override string ToString()
        {
            return String.Format("ReleaseWindowId={0}, CommandContext={1}",  ReleaseWindowId,
                CommandContext);
        }
    }
}
