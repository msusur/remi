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
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.DataAccess.BusinessEntityGateways.Subscriptions;
using ReMi.Events.ReleaseCalendar;

namespace ReMi.EventHandlers.ReleaseCalendar
{
    public class UpdateWindowShouldNotifyTeamMembersEventHandler : IHandleEvent<ReleaseWindowUpdatedEvent>
    {
        public Func<IAccountNotificationGateway> AccountNotificationGatewayFactory { get; set; }
        public Func<IReleaseParticipantGateway> ReleaseParticipantGatewayFactory { get; set; }
        public IEmailTextProvider EmailTextProvider { get; set; }
        public IEmailService EmailService { get; set; }

        public void Handle(ReleaseWindowUpdatedEvent evnt)
        {
            List<Account> participants;
            using (var participantGateway = ReleaseParticipantGatewayFactory())
            {
                participants =
                    participantGateway.GetReleaseParticipants(evnt.ReleaseWindow.ExternalId)
                        .Select(x => x.Account)
                        .ToList();
            }

            var productsJoined = evnt.ReleaseWindow.Products.FormatElements(string.Empty, string.Empty);
            List<Account> subscribers;
            String windowBookedSubject = String.Format(
                "{0} {1} {2}",
                productsJoined,
                EnumDescriptionHelper.GetDescription(evnt.ReleaseWindow.ReleaseType),
                "Window Was updated");

            using (var gateway = AccountNotificationGatewayFactory())
            {
                subscribers =
                    gateway.GetSubscribers(NotificationType.ReleaseWindowsSchedule, evnt.ReleaseWindow.Products)
                        .Where(x => participants.All(p => x.ExternalId != p.ExternalId))
                        .ToList();
            }

            foreach (var subscriber in subscribers)
            {
                var replaceValues = new Dictionary<string, object>
                {
                    {"Sprint", evnt.ReleaseWindow.Sprint},
                    {"Products", productsJoined},
                    {"ReleaseType", EnumDescriptionHelper.GetDescription(evnt.ReleaseWindow.ReleaseType)},
                    {"StartTime", String.Format("{0:dd/MM/yyyy HH:mm}", evnt.ReleaseWindow.StartTime.ToLocalTime())},
                    {"Assignee", subscriber.FullName},
                    {"ReleasePlanUrl", string.Format("{0}release?{1}={2}", ConfigurationManager.AppSettings["frontendUrl"], "releaseWindowId", evnt.ReleaseWindow.ExternalId)},
                };

                var email = EmailTextProvider.GetText("ReleaseWindowUpdatedEmail", replaceValues);

                EmailService.Send(subscriber.Email, windowBookedSubject, email);
            }
        }
    }
}
