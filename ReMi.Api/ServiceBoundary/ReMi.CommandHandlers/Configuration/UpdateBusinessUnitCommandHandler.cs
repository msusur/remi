using System;
using ReMi.BusinessEntities.Products;
using ReMi.Commands.Configuration;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.Events.Configuration;

namespace ReMi.CommandHandlers.Configuration
{
    public class UpdateBusinessUnitCommandHandler : IHandleCommand<UpdateBusinessUnitCommand>
    {
        public Func<IBusinessUnitsGateway> BusinessUnitsGatewayFactory { get; set; }
        public IPublishEvent EventPublisher { get; set; }

        public void Handle(UpdateBusinessUnitCommand command)
        {
            using (var gateway = BusinessUnitsGatewayFactory())
            {
                gateway.UpdateBusinessUnit(new BusinessUnit
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
