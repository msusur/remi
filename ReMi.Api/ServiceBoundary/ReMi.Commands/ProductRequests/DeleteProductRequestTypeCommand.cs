using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ProductRequests
{
    [Command("Delete product request type", CommandGroup.ProductRequest)]
    public class DeleteProductRequestTypeCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Guid RequestTypeId { get; set; }

        public override string ToString()
        {
            return string.Format("[RequestTypeId={0}, CommandContext={1}]",
                RequestTypeId, CommandContext);
        }
    }
}
