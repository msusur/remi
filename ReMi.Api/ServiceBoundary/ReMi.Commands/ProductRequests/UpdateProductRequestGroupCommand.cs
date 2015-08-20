using ReMi.BusinessEntities.ProductRequests;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ProductRequests
{
    [Command("Update product request group", CommandGroup.ProductRequest)]
    public class UpdateProductRequestGroupCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public ProductRequestGroup RequestGroup { get; set; }

        public override string ToString()
        {
            return string.Format("[RequestGroup={0}, CommandContext={1}]",
                RequestGroup, CommandContext);
        }
    }
}
