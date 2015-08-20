using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessLogic;
using ReMi.Common.Constants.Subscriptions;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Plugins.Services.Email;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.Subscriptions;
using ReMi.DataAccess.Exceptions;
using ReMi.Events.ReleasePlan;

namespace ReMi.EventHandlers.ReleasePlan
{
    public class ApproverAddedToReleaseWindowEmailNotificationHandler : IHandleEvent<ApproverAddedToReleaseWindowEvent>
    {
        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }
        public Func<IAccountNotificationGateway> AccountNotificationGatewayFactory { get; set; }
        public IEmailService EmailService { get; set; }
        public IEmailTextProvider EmailTextProvider { get; set; }

        private const string ApproverAddedToReleaseSubject = "{0} Added to approvers";

        public void Handle(ApproverAddedToReleaseWindowEvent evnt)
        {
            ReleaseWindow releaseWindow;
            using (var gateway = ReleaseWindowGatewayFactory())
            {
                releaseWindow = gateway.GetByExternalId(evnt.ReleaseWindowId);
                if (releaseWindow == null)
                    throw new ReleaseWindowNotFoundException(evnt.ReleaseWindowId);
            }

            using (var notificationGateway = AccountNotificationGatewayFactory())
            {
                var subscriber = notificationGateway.GetSubscribers(NotificationType.Approvement, releaseWindow.Products)
                    .FirstOrDefault(x => x.ExternalId == evnt.Approver.Account.ExternalId);

                if (subscriber == null)
                {
                    return;
                }

                var replaceValues = new Dictionary<string, object>
                {
                    {"Assignee", subscriber.FullName},
                    {"Products", releaseWindow.Products.FormatElements(string.Empty, string.Empty)},
                    {"Sprint", releaseWindow.Sprint},
                    {"StartTime", String.Format("{0:dd/MM/yyyy HH:mm}", releaseWindow.StartTime.ToLocalTime())},
                    {
                        "ReleasePlanUrl",
                        string.Format("{0}release?{1}={2}", ConfigurationManager.AppSettings["frontendUrl"],
                            "releaseWindowId", releaseWindow.ExternalId)
                    },
                };

                var email = EmailTextProvider.GetText("ApproverAddedToReleaseWindowEmail", replaceValues);

                EmailService.Send(subscriber.Email,
                    string.Format(ApproverAddedToReleaseSubject, releaseWindow.Products.FormatElements(string.Empty, string.Empty)),
                    email);
            }
        }
    }
}
