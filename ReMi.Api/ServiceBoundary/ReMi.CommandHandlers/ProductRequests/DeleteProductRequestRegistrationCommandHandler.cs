using System;
using ReMi.Commands.ProductRequests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.ProductRequests;

namespace ReMi.CommandHandlers.ProductRequests
{
    public class DeleteProductRequestRegistrationCommandHandler : IHandleCommand<DeleteProductRequestRegistrationCommand>
    {
        public Func<IProductRequestRegistrationGateway> ProductRequestRegistrationGatewayFactory { get; set; }

        public void Handle(DeleteProductRequestRegistrationCommand command)
        {
            using (var gateway = ProductRequestRegistrationGatewayFactory())
            {
                gateway.DeleteProductRequestRegistration(command.RegistrationId, command.RemovingReason, command.Comment);
            }
        }
    }
}
