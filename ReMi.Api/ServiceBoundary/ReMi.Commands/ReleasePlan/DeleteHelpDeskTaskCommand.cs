using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ReleasePlan
{
    [Command("Delete Help Desk Task", CommandGroup.ReleaseTask, IsBackground = true)]
    public class DeleteHelpDeskTaskCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Guid ReleaseWindowId { get; set; }
        public string HelpDeskTicketRef { get; set; }
    }
} ;
