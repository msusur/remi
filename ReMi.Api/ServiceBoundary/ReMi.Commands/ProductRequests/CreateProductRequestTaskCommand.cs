using ReMi.BusinessEntities.ProductRequests;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ProductRequests
{
    [Command("Create product request task", CommandGroup.ProductRequest)]
    public class CreateProductRequestTaskCommand : ICommand
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
