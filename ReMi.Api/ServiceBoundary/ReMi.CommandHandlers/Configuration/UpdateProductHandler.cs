using System;
using AutoMapper;
using ReMi.BusinessEntities.Products;
using ReMi.Commands.Configuration;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.Events.Configuration;

namespace ReMi.CommandHandlers.Configuration
{
    public class UpdateProductHandler : IHandleCommand<UpdateProductCommand>
    {
        public Func<IProductGateway> ProductGatewayFactory { get; set; }
        public IPublishEvent EventPublisher { get; set; }
        public IMappingEngine MappingEngine { get; set; }

        public void Handle(UpdateProductCommand command)
        {
            var product = MappingEngine.Map<UpdateProductCommand, Product>(command);
            using (var gateway = ProductGatewayFactory())
            {
                gateway.UpdateProduct(product);
            }

            EventPublisher.Publish(new BusinessUnitsChangedEvent());
        }
    }
}
