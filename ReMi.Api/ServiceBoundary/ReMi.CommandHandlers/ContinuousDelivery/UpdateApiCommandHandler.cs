using System;
using System.Linq;
using ReMi.Commands.ContinuousDelivery;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Api;
using ReMi.Events.Api;

namespace ReMi.CommandHandlers.ContinuousDelivery
{
    public class UpdateApiCommandHandler : IHandleCommand<UpdateApiCommand>
    {
        public Func<IApiDescriptionGateway> ApiDescriptionGatewayFactory { get; set; }
        public IPublishEvent EventPublisher { get; set; }

        public void Handle(UpdateApiCommand command)
        {
            using (var gateway = ApiDescriptionGatewayFactory())
            {
                var storedDescriptions = gateway.GetApiDescriptions();

                var descToRemove =
                    storedDescriptions.Where(d => !command.ApiDescriptions.Any(c => c.Method == d.Method && c.Url == d.Url)).ToList();

                var descToAdd =
                    command.ApiDescriptions.Where(d => !storedDescriptions.Any(c => c.Method == d.Method && c.Url == d.Url)).ToList();

                var updatedEvent = new ApiUpdatedEvent();

                if (descToRemove.Any())
                {
                    gateway.RemoveApiDescriptions(descToRemove);
                    updatedEvent.RemovedDescriptions = descToRemove;
                }

                if (descToAdd.Any())
                {
                    gateway.CreateApiDescriptions(descToAdd);
                    updatedEvent.AddedDescriptions = descToAdd;
                }

                if ((updatedEvent.AddedDescriptions != null && updatedEvent.AddedDescriptions.Any())
                    || (updatedEvent.RemovedDescriptions != null && updatedEvent.RemovedDescriptions.Any()))
                {
                    EventPublisher.Publish(updatedEvent);
                }
            }
        }
    }
}
