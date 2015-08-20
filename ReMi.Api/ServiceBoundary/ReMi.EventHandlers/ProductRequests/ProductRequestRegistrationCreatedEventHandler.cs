using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Common.Logging;
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
    public class ProductRequestRegistrationCreatedEventHandler : IHandleEvent<ProductRequestRegistrationCreatedEvent>
    {
        public Func<IProductRequestRegistrationGateway> ProductRequestRegistrationGatewayFactory { get; set; }
        public Func<IProductRequestGateway> ProductRequestGatewayFactory { get; set; }
        public IEmailTextProvider EmailTextProvider { get; set; }
        public IEmailService EmailService { get; set; }

        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();


        public void Handle(ProductRequestRegistrationCreatedEvent evnt)
        {
            var entireRegistration = GetDataRegistration(evnt.Registration.ExternalId);

            using (var gateway = ProductRequestGatewayFactory())
            {
                var type = gateway.GetRequestType(entireRegistration.ProductRequestTypeId);
                if (type == null)
                    throw new EntityNotFoundException(typeof(ProductRequestType), entireRegistration.ProductRequestTypeId);

                if (type.RequestGroups == null)
                {
                    Logger.DebugFormat("Request don't has groups. RequstTypeId={0}", entireRegistration.ProductRequestTypeId);
                    return;
                }

                var groups = type.RequestGroups.ToList();

                foreach (var requestGroup in groups)
                {
                    if (requestGroup.Assignees != null)
                    {
                        var currentTasks = ProductRequestRegistrationTaskLayoutGenerator.Get(
                            entireRegistration,
                            requestGroup.RequestTasks.OrderBy(o => o.Question).ToList());

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

                            var email = EmailTextProvider.GetText("ProductRequestRegistrationCreated", replaceValues);

                            EmailService.Send(assignee.Email, "Product Registration Created", email);
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
