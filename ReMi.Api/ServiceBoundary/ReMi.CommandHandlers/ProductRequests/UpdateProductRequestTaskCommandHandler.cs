using System;
using ReMi.Commands.ProductRequests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.ProductRequests;

namespace ReMi.CommandHandlers.ProductRequests
{
    public class UpdateProductRequestTaskCommandHandler : IHandleCommand<UpdateProductRequestTaskCommand>
    {
        public Func<IProductRequestGateway> ProductRequestGatewayFactory { get; set; }

        public void Handle(UpdateProductRequestTaskCommand command)
        {
            using (var gateway = ProductRequestGatewayFactory())
            {
                gateway.UpdateProductRequestTask(command.RequestTask);
            }
        }
    }
}
