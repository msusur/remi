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
using ReMi.Events.ReleaseCalendar;

namespace ReMi.EventHandlers.ReleaseCalendar
{
    public class ReleaseWindowApprovedEventHandler : IHandleEvent<ReleaseWindowApprovedEvent>
    {
        public IEmailService EmailService { get; set; }
        public IEmailTextProvider EmailTextProvider { get; set; }
        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }
        public Func<IAccountNotificationGateway> AccountNotificationGatewayFactory { get; set; }
        public Func<IReleaseApproverGateway> ReleaseApproverGatewayFactory { get; set; }

        public void Handle(ReleaseWindowApprovedEvent evnt)
        {
            if (evnt.ReleaseWindow == null)
                throw new ArgumentException("ReleaseWindow is not initialized");

            ReleaseWindow releaseWindow;
            using (var gateway = ReleaseWindowGatewayFactory())
            {
                releaseWindow = gateway.GetByExternalId(evnt.ReleaseWindow.ExternalId);
                if (releaseWindow == null)
                    throw new ReleaseWindowNotFoundException(evnt.ReleaseWindow.ExternalId);
            }

            IEnumerable<string> addressList;
            using (var gateway = AccountNotificationGatewayFactory())
            {
                addressList = gateway.GetSubscribers(NotificationType.Approvement, evnt.ReleaseWindow.Products)
                    .Select(o => o.Email)
                    .ToList();
            }

            if (!addressList.Any())
                return;

            using (var gateway = ReleaseApproverGatewayFactory())
            {
                var approvers = gateway.GetApprovers(evnt.ReleaseWindow.ExternalId).ToList();

                var signedBy = approvers
                    .Select(o => new { o.Account.FullName, o.ApprovedOn, ApprovementDescriprion = o.Comment })
                    .Select(
                        o => string.Format("<tr><td style='padding-right: 2em'>{0}</td><td>{1}</td><td>{2}</td></tr>",
                            o.FullName,
                            o.ApprovedOn.HasValue
                                ? "approved at " + o.ApprovedOn.Value.ToLocalTime().ToShortDateString() + " " +
                                  o.ApprovedOn.Value.ToLocalTime().ToShortTimeString()
                                : "-",
                            o.ApprovementDescriprion))
                    .ToArray();

                var replaceValues = new Dictionary<string, object>
                {
                    {"Products", releaseWindow.Products.FormatElements(string.Empty, string.Empty)},
                    {"Sprint", releaseWindow.Sprint},
                    {"ListOfApprovers", string.Format("<table style='border: none'>{0}</table>", string.Join("", signedBy))},
                    {"StartTime", string.Format("{0:dd/MM/yyyy HH:mm}", releaseWindow.StartTime.ToLocalTime())},
                    {"ReleasePlanUrl", string.Format("{0}release?{1}={2}", ConfigurationManager.AppSettings["frontendUrl"], "releaseWindowId", releaseWindow.ExternalId)},
                };

                var email = EmailTextProvider.GetText("ReleaseWindowFullyApprovedEmail", replaceValues);

                EmailService.Send(addressList,
                    string.Format("{0} Release approved", releaseWindow.Products.FormatElements(string.Empty, string.Empty)),
                    email);
            }
        }
    }
}

