using System;
using ReMi.Commands.ProductRequests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.ProductRequests;

namespace ReMi.CommandHandlers.ProductRequests
{
    public class DeleteProductRequestTypeCommandHandler : IHandleCommand<DeleteProductRequestTypeCommand>
    {
        public Func<IProductRequestGateway> ProductRequestGatewayFactory { get; set; }

        public void Handle(DeleteProductRequestTypeCommand command)
        {
            using (var gateway = ProductRequestGatewayFactory())
            {
                gateway.DeleteProductRequestType(command.RequestTypeId);
            }
        }
    }
}
