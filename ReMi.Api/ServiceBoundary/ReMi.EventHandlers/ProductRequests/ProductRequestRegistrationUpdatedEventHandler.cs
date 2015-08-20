using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using ReMi.BusinessEntities.ProductRequests;
using ReMi.BusinessLogic;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Plugins.Services.Email;
using ReMi.DataAccess.BusinessEntityGateways.ProductRequests;
using ReMi.DataAccess.Exceptions;
using ReMi.EventHandlers.Helpers;
using ReMi.Events.ProductRequests;

namespace ReMi.EventHandlers.ProductRequests
{
    public class ProductRequestRegistrationUpdatedEventHandler : IHandleEvent<ProductRequestRegistrationUpdatedEvent>
    {
        public Func<IProductRequestRegistrationGateway> ProductRequestRegistrationGatewayFactory { get; set; }
        public Func<IProductRequestGateway> ProductRequestGatewayFactory { get; set; }
        public IEmailTextProvider EmailTextProvider { get; set; }
        public IEmailService EmailService { get; set; }

        public void Handle(ProductRequestRegistrationUpdatedEvent evnt)
        {
            if (evnt.ChangedTasks == null)
                throw new ArgumentException("ChangedTasks collection is not initialized");

            var entireRegistration = GetDataRegistration(evnt.Registration.ExternalId);

            using (var gateway = ProductRequestGatewayFactory())
            {
                var groups = gateway.GetRequestGroupsByTasks(evnt.ChangedTasks);

                foreach (var requestGroup in groups)
                {
                    if (requestGroup.Assignees != null)
                    {
                        var currentTasks = ProductRequestRegistrationTaskLayoutGenerator.Get(
                            entireRegistration,
                            requestGroup.RequestTasks.OrderBy(o => o.Question).ToList(),
                            evnt.ChangedTasks);

                        var assignees = requestGroup.Assignees.ToList();

                        foreach (var assignee in assignees)
                        {
                            var replaceValues = new Dictionary<string, object>
                            {
                                {"Assignee", assignee.FullName},
                                {"Description", evnt.Registration.Description},
                                {"Tasks", currentTasks },
                                {"RegistrationUrl", string.Format("{0}productRegistration", ConfigurationManager.AppSettings["frontendUrl"])},
                            };

                            var email = EmailTextProvider.GetText("ProductRequestRegistrationUpdated", replaceValues);

                            EmailService.Send(assignee.Email, "Product Registration Updated", email);
                        }
                    }
                }
            }
        }

        private ProductRequestRegistration GetDataRegistration(Guid registrationId)
        {
            using (var gateway = ProductRequestRegistrationGatewayFactory())
            {
                var registration = gateway.GetRegistration(registrationId);

                if (registration == null)
                    throw new EntityNotFoundException(typeof(ProductRequestRegistration), registrationId);

                return registration;
            }
        }
    }
}
