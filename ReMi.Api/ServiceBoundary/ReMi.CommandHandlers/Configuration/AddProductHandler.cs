using System;
using AutoMapper;
using ReMi.BusinessEntities.Products;
using ReMi.Commands.Configuration;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.Events.Configuration;
using ReMi.Events.Packages;

namespace ReMi.CommandHandlers.Configuration
{
    public class AddProductHandler : IHandleCommand<AddProductCommand>
    {
        public Func<IProductGateway> ProductGatewayFactory { get; set; }
        public Func<IAccountsGateway> AccountsGateway { get; set; }
        public IPublishEvent EventPublisher { get; set; }
        public IMappingEngine MappingEngine { get; set; }

        public void Handle(AddProductCommand command)
        {
            var product = MappingEngine.Map<AddProductCommand, Product>(command);
            using (var gateway = ProductGatewayFactory())
            {
                gateway.AddProduct(product);
            }
            using (var gateway = AccountsGateway())
            {
                gateway.AssociateAccountsWithProducts(new[] { product.ExternalId }, new[] { command.CommandContext.UserEmail });
            }

            EventPublisher.Publish(new BusinessUnitsChangedEvent { Context = command.CommandContext.CreateChild<EventContext>() });
            EventPublisher.Publish(new NewPackageAddedEvent
            {
                Package = product,
                Context = command.CommandContext.CreateChild<EventContext>()
            });
        }
    }
}
