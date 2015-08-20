using System;
using ReMi.Commands.ProductRequests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.ProductRequests;

namespace ReMi.CommandHandlers.ProductRequests
{
    public class DeleteProductRequestGroupCommandHandler : IHandleCommand<DeleteProductRequestGroupCommand>
    {
        public Func<IProductRequestGateway> ProductRequestGatewayFactory { get; set; }

        public void Handle(DeleteProductRequestGroupCommand command)
        {
            using (var gateway = ProductRequestGatewayFactory())
            {
                gateway.DeleteProductRequestGroup(command.RequestGroupId);
            }
        }
    }
}
