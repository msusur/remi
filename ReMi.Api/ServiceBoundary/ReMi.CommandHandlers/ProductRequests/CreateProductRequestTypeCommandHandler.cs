using System;
using ReMi.Commands.ProductRequests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.ProductRequests;

namespace ReMi.CommandHandlers.ProductRequests
{
    public class CreateProductRequestTypeCommandHandler : IHandleCommand<CreateProductRequestTypeCommand>
    {
        public Func<IProductRequestGateway> ProductRequestGatewayFactory { get; set; }

        public void Handle(CreateProductRequestTypeCommand command)
        {
            using (var gateway = ProductRequestGatewayFactory())
            {
                gateway.CreateProductRequestType(command.RequestType);
            }
        }
    }
}
