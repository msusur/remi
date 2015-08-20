using ReMi.BusinessEntities.ProductRequests;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ProductRequests
{
    [Command("Update product request type", CommandGroup.ProductRequest)]
    public class UpdateProductRequestTypeCommand : ICommand
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
