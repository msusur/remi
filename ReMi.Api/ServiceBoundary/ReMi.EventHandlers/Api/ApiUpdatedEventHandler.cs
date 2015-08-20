using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using ReMi.BusinessEntities.Api;
using ReMi.BusinessLogic;
using ReMi.Common.Constants.Subscriptions;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Plugins.Services.Email;
using ReMi.DataAccess.BusinessEntityGateways.Subscriptions;
using ReMi.Events.Api;

namespace ReMi.EventHandlers.Api
{
    public class ApiUpdatedEventHandler : IHandleEvent<ApiUpdatedEvent>
    {
        public Func<IAccountNotificationGateway> AccountNotificationGatewayFactory { get; set; }
        public IEmailService EmailService { get; set; }
        public IEmailTextProvider EmailTextProvider { get; set; }

        private const string EmailSubject = "ReMi API update";

        public void Handle(ApiUpdatedEvent evnt)
        {
            using (var gateway = AccountNotificationGatewayFactory())
            {
                var subscribers = gateway.GetSubscribers(NotificationType.ApiChange);

                if (!subscribers.Any())
                {
                    return;
                }

                var email = EmailTextProvider.GetText("ApiUpdated", new Dictionary<string, object>
                {
                    {"AddedMethods", FormatDescriptionsEmail(evnt.AddedDescriptions, "Added API methods:")},
                    {"RemovedMethods", FormatDescriptionsEmail(evnt.RemovedDescriptions, "Removed API methods:")},
                    {
                        "RemiApiPageUrl",
                        String.Format("{0}{1}", ConfigurationManager.AppSettings["frontendUrl"], "apiDescription")
                    },
                });

                EmailService.Send(subscribers.Select(x => x.Email), EmailSubject, email.ToString());
            }
        }

        private string FormatDescriptionsEmail(List<ApiDescription> descriptions, String title)
        {
            if (descriptions==null || !descriptions.Any())
            {
                return String.Empty;
            }

            var email = new StringBuilder(title);
            email.AppendLine("<table><tr><td style=\"margin-right:10px;\">HttpMethod</td><td>Url</td></tr>");
            foreach (var desc in descriptions)
            {
                email.AppendLine(String.Format("<tr><td>{0}</td><td>{1}</td></tr>", desc.Method, desc.Url));
            }
            email.AppendLine("</table>");

            return email.ToString();
        }
    }
}
