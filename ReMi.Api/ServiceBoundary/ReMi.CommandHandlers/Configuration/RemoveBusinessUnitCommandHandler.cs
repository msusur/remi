using System;
using ReMi.Commands.Configuration;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.Events.Configuration;

namespace ReMi.CommandHandlers.Configuration
{
    public class RemoveBusinessUnitCommandHandler : IHandleCommand<RemoveBusinessUnitCommand>
    {
        public Func<IBusinessUnitsGateway> BusinessUnitsGatewayFactory { get; set; }
        public IPublishEvent EventPublisher { get; set; }

        public void Handle(RemoveBusinessUnitCommand command)
        {
            using (var gateway = BusinessUnitsGatewayFactory())
            {
                gateway.RemoveBusinessUnit(command.ExternalId);
            }

            EventPublisher.Publish(new BusinessUnitsChangedEvent());
        }
    }
}
