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
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.DataAccess.BusinessEntityGateways.Subscriptions;
using ReMi.DataAccess.Exceptions;
using ReMi.Events.ReleasePlan;

namespace ReMi.EventHandlers.ReleasePlan
{
    public class ApproverRemovedFromReleaseWindowEmailNotificationHandler :
        IHandleEvent<ApproverRemovedFromReleaseEvent>
    {
        public IEmailService EmailService { get; set; }
        public IEmailTextProvider EmailTextProvider { get; set; }
        public Func<IAccountNotificationGateway> AccountNotificationGatewayFactory { get; set; }
        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }
        public Func<IReleaseApproverGateway> ReleaseApproverGatewayFactory { get; set; }

        private const string ApproverRemovedFromReleaseSubject = "{0} Removed from approvers";

        public void Handle(ApproverRemovedFromReleaseEvent evnt)
        {
            ReleaseWindow releaseWindow;
            using (var gateway = ReleaseWindowGatewayFactory())
            {
                releaseWindow = gateway.GetByExternalId(evnt.ReleaseWindowId);
                if (releaseWindow == null)
                    throw new ReleaseWindowNotFoundException(evnt.ReleaseWindowId);
            }

            using (var gateway = AccountNotificationGatewayFactory())
            {
                using (var approversGateway = ReleaseApproverGatewayFactory())
                {
                    var approver = approversGateway.GetApprovers(evnt.ReleaseWindowId)
                        .FirstOrDefault(x => x.ExternalId == evnt.ApproverId);

                    if (approver == null)
                        throw new ReleaseApproverNotFoundException(evnt.ApproverId);

                    var subscriber = gateway.GetSubscribers(NotificationType.Approvement, releaseWindow.Products)
                        .FirstOrDefault(x => x.ExternalId == approver.Account.ExternalId);

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
                            String.Format("{0}release?{1}={2}", ConfigurationManager.AppSettings["frontendUrl"],
                                "releaseWindowId", releaseWindow.ExternalId)
                        },
                    };

                    var email = EmailTextProvider.GetText("ApproverRemovedFromReleaseWindowEmail", replaceValues);

                    EmailService.Send(subscriber.Email,
                        string.Format(ApproverRemovedFromReleaseSubject, releaseWindow.Products.FormatElements(string.Empty, string.Empty)),
                        email);
                }

            }
        }
    }
}
