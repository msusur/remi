using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ProductRequests
{
    [Command("Delete product request task", CommandGroup.ProductRequest)]
    public class DeleteProductRequestTaskCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Guid RequestTaskId { get; set; }

        public override string ToString()
        {
            return string.Format("[RequestTaskId={0}, CommandContext={1}]",
                RequestTaskId, CommandContext);
        }
    }
}
