using ReMi.BusinessEntities.ProductRequests;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.ProductRequests
{
    [Command("Create product request registration", CommandGroup.ProductRequest)]
    public class CreateProductRequestRegistrationCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public ProductRequestRegistration Registration { get; set; }

        public override string ToString()
        {
            return string.Format("[Registration={0}, CommandContext={1}]", Registration, CommandContext);
        }
    }
}
