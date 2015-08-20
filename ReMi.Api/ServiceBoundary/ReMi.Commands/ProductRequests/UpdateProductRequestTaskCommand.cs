using ReMi.BusinessEntities.ProductRequests;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ProductRequests
{
    [Command("Update product request group", CommandGroup.ProductRequest)]
    public class UpdateProductRequestTaskCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public ProductRequestTask RequestTask { get; set; }

        public override string ToString()
        {
            return string.Format("[RequestTask={0}, CommandContext={1}]",
                RequestTask, CommandContext);
        }
    }
}
