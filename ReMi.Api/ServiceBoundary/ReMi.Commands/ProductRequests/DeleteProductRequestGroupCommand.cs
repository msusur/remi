using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ProductRequests
{
    [Command("Delete product request group", CommandGroup.ProductRequest)]
    public class DeleteProductRequestGroupCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Guid RequestGroupId { get; set; }

        public override string ToString()
        {
            return string.Format("[RequestGroupId={0}, CommandContext={1}]",
                RequestGroupId, CommandContext);
        }
    }
}
