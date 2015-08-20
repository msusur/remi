using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessLogic;
using ReMi.Common.Constants.Subscriptions;
using ReMi.Common.Utils;
using ReMi.Common.Utils.Enums;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Plugins.Services.Email;
using ReMi.DataAccess.BusinessEntityGateways.Subscriptions;
using ReMi.Events.ReleaseCalendar;

namespace ReMi.EventHandlers.ReleaseCalendar
{
    public class ReleaseWindowBookedEventHandlerEmailNotification : IHandleEvent<ReleaseWindowBookedEvent>
    {
        public Func<IAccountNotificationGateway> AccountNotificationGatewayFactory { get; set; }
        public IEmailTextProvider EmailTextProvider { get; set; }
        public IEmailService EmailService { get; set; }

        public void Handle(ReleaseWindowBookedEvent evnt)
        {
            var products = evnt.ReleaseWindow.Products.FormatElements(string.Empty, string.Empty);
            
            String windowBookedSubject = String.Format("{0} {1} {2}",
                products, 
                EnumDescriptionHelper.GetDescription(evnt.ReleaseWindow.ReleaseType), 
                "Window Was Booked");

            List<Account> teamMembers;
            using (var gateway = AccountNotificationGatewayFactory())
            {
                teamMembers =
                    gateway.GetSubscribers(NotificationType.ReleaseWindowsSchedule, evnt.ReleaseWindow.Products).ToList();
            }

            foreach (var teamMember in teamMembers)
            {
                var replaceValues = new Dictionary<string, object>
                {
                    {"Sprint", evnt.ReleaseWindow.Sprint},
                    {"Products", products},
                    {"ReleaseType", EnumDescriptionHelper.GetDescription(evnt.ReleaseWindow.ReleaseType) },
                    {"StartTime", String.Format("{0:dd/MM/yyyy HH:mm}", evnt.ReleaseWindow.StartTime.ToLocalTime())},
                    {"Assignee", teamMember.FullName},
                    {"ReleasePlanUrl", string.Format("{0}release?{1}={2}", ConfigurationManager.AppSettings["frontendUrl"], "releaseWindowId", evnt.ReleaseWindow.ExternalId)},
                };

                var email = EmailTextProvider.GetText("ReleaseWindowBookedEmail", replaceValues);

                EmailService.Send(teamMember.Email, windowBookedSubject, email);
            }
        }
    }
}
