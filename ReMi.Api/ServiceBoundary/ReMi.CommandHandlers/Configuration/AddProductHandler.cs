using System;
using System.Collections.Generic;
using AutoMapper;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.Products;
using ReMi.BusinessLogic.BusinessRules;
using ReMi.Commands.Configuration;
using ReMi.Common.Constants.BusinessRules;
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
        public IBusinessRuleEngine BusinessRuleEngine { get; set; }

        public void Handle(AddProductCommand command)
        {
            var product = MappingEngine.Map<AddProductCommand, Product>(command);
            using (var gateway = ProductGatewayFactory())
            {
                gateway.AddProduct(product);
            }
            using (var gateway = AccountsGateway())
            {
                gateway.AssociateAccountsWithProducts(new[] { product.ExternalId }, new[] { command.CommandContext.UserEmail }, GetRule(command.CommandContext.UserId));
            }

            EventPublisher.Publish(new BusinessUnitsChangedEvent { Context = command.CommandContext.CreateChild<EventContext>() });
            EventPublisher.Publish(new NewPackageAddedEvent
            {
                Package = product,
                Context = command.CommandContext.CreateChild<EventContext>()
            });
        }

        private Func<string, TeamRoleRuleResult> GetRule(Guid userId)
        {
            return s => BusinessRuleEngine.Execute<TeamRoleRuleResult>(
                userId, BusinessRuleConstants.Config.TeamRoleRule.ExternalId,
                new Dictionary<string, object> { { "roleName", s } });
        }
    }
}
