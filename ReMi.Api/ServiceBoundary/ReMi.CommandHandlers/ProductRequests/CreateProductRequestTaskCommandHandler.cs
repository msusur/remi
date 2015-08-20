using System;
using ReMi.Commands.ProductRequests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.ProductRequests;

namespace ReMi.CommandHandlers.ProductRequests
{
    public class CreateProductRequestTaskCommandHandler : IHandleCommand<CreateProductRequestTaskCommand>
    {
        public Func<IProductRequestGateway> ProductRequestGatewayFactory { get; set; }

        public void Handle(CreateProductRequestTaskCommand command)
        {
            using (var gateway = ProductRequestGatewayFactory())
            {
                gateway.CreateProductRequestTask(command.RequestTask);
            }
        }
    }
}
