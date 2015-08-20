using System;
using ReMi.Commands.ProductRequests;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ProductRequests;
using ReMi.Events.ProductRequests;

namespace ReMi.CommandHandlers.ProductRequests
{
    public class CreateProductRequestRegistrationCommandHandler : IHandleCommand<CreateProductRequestRegistrationCommand>
    {
        public Func<IProductRequestRegistrationGateway> ProductRequestRegistrationGatewayFactory { get; set; }
        public IPublishEvent PublishEvent { get; set; }

        public void Handle(CreateProductRequestRegistrationCommand command)
        {
            using (var gateway = ProductRequestRegistrationGatewayFactory())
            {
                command.Registration.CreatedByAccountId = command.CommandContext.UserId;

                gateway.CreateProductRequestRegistration(command.Registration);

                PublishEvent.Publish(
                    new ProductRequestRegistrationCreatedEvent
                    {
                        Context = command.CommandContext.CreateChild<EventContext>(),
                        Registration = command.Registration
                    });
            }
        }
    }
}
