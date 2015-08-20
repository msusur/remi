using ReMi.BusinessEntities.ProductRequests;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ProductRequests
{
    [Command("Create product request type", CommandGroup.ProductRequest)]
    public class CreateProductRequestTypeCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public ProductRequestType RequestType { get; set; }

        public override string ToString()
        {
            return string.Format("[RequestType={0}, CommandContext={1}]",
                RequestType, CommandContext);
        }
    }
}
