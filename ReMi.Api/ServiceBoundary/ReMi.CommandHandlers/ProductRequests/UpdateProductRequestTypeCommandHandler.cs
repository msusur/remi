using System;
using ReMi.Commands.ProductRequests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.ProductRequests;

namespace ReMi.CommandHandlers.ProductRequests
{
    public class UpdateProductRequestTypeCommandHandler : IHandleCommand<UpdateProductRequestTypeCommand>
    {
        public Func<IProductRequestGateway> ProductRequestGatewayFactory { get; set; }

        public void Handle(UpdateProductRequestTypeCommand command)
        {
            using (var gateway = ProductRequestGatewayFactory())
            {
                gateway.UpdateProductRequestType(command.RequestType);
            }
        }
    }
}
