using System;
using ReMi.BusinessEntities.Products;
using ReMi.Commands.Configuration;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.Events.Configuration;

namespace ReMi.CommandHandlers.Configuration
{
    public class AddBusinessUnitCommandHandler : IHandleCommand<AddBusinessUnitCommand>
    {
        public Func<IBusinessUnitsGateway> BusinessUnitsGatewayFactory { get; set; }
        public IPublishEvent EventPublisher { get; set; }

        public void Handle(AddBusinessUnitCommand command)
        {
            using (var gateway = BusinessUnitsGatewayFactory())
            {
                gateway.AddBusinessUnit(new BusinessUnit
                {
                    Description = command.Description,
                    Name = command.Name,
                    ExternalId = command.ExternalId
                });
            }

            EventPublisher.Publish(new BusinessUnitsChangedEvent());
        }
    }
}
