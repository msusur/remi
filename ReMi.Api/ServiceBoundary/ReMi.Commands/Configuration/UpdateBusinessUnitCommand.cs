using System;
using ReMi.BusinessEntities.Products;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.Configuration
{
    [Command("Update existing business unit", CommandGroup.Configuration)]
    public class UpdateBusinessUnitCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }
        public Product Product { get; set; }

        public override string ToString()
        {
            return String.Format("Product={0}, CommandContext={1}", Product, CommandContext);
        }
    }
}
