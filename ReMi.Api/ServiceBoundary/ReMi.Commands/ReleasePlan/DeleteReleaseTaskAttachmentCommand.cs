using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleasePlan
{
    [Command("Remove Task Attachement", CommandGroup.ReleaseTask, IsBackground = true)]
    public class DeleteReleaseTaskAttachmentCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Guid ReleaseTaskAttachmentId { get; set; }

        public override string ToString()
        {
            return string.Format("[CommandContext = {0}, ReleaseTaskAttachmentId = {1}]",
                CommandContext, ReleaseTaskAttachmentId);
        }
    }
}
