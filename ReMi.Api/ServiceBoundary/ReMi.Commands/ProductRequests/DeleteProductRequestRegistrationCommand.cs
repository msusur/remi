using System;
using ReMi.Common.Constants.ProductRequests;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ProductRequests
{
    [Command("Delete product request registration", CommandGroup.ProductRequest)]
    public class DeleteProductRequestRegistrationCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Guid RegistrationId { get; set; }

        public RemovingReason RemovingReason { get; set; }

        public string Comment { get; set; }

        public override string ToString()
        {
            return string.Format("[RegistrationId={0}, RemovingReason={1}, Comment={2}, CommandContext={3}]",
                RegistrationId, RemovingReason, Comment, CommandContext);
        }
    }
}
