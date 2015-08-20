using System;
using ReMi.Commands.ProductRequests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.ProductRequests;

namespace ReMi.CommandHandlers.ProductRequests
{
    public class DeleteProductRequestTaskCommandHandler : IHandleCommand<DeleteProductRequestTaskCommand>
    {
        public Func<IProductRequestGateway> ProductRequestGatewayFactory { get; set; }

        public void Handle(DeleteProductRequestTaskCommand command)
        {
            using (var gateway = ProductRequestGatewayFactory())
            {
                gateway.DeleteProductRequestTask(command.RequestTaskId);
            }
        }
    }
}
