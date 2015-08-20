using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.BusinessLogic;
using ReMi.Common.Constants.Subscriptions;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Plugins.Services.Email;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.Subscriptions;
using ReMi.Events.ReleasePlan;

namespace ReMi.EventHandlers.ReleasePlan
{
    public class ReleaseTaskUpdatedEmailNotificationHandler : IHandleEvent<ReleaseTaskUpdatedEvent>
    {
        public IEmailTextProvider EmailTextProvider { get; set; }
        public IEmailService EmailService { get; set; }
        public Func<IAccountsGateway> AccountsGatewayFactory { get; set; }
        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }
        public Func<IAccountNotificationGateway> AccountNotificationGatewayFactory { get; set; }

        public void Handle(ReleaseTaskUpdatedEvent evnt)
        {
            if(evnt == null)
                throw new ArgumentNullException("evnt");

            var releaseTask = evnt.ReleaseTask;
            if (releaseTask == null)
                throw new ArgumentException("Release Task is not initialized");

            if (releaseTask.AssigneeExternalId == null || releaseTask.AssigneeExternalId == Guid.Empty)
                throw new ArgumentException(String.Format("Assignee for release task is missing. ReleaseTask={0}", releaseTask));

            var releaseWindow = GetReleaseWindow(releaseTask.ReleaseWindowId);
            var assignee = GetAccount(releaseTask.AssigneeExternalId);
            
            NotifyAssignee(releaseTask, releaseWindow, assignee);

            NotifySupportSubscribers(releaseTask, releaseWindow, assignee);
        }

        private void NotifyAssignee(ReleaseTask releaseTask, ReleaseWindow releaseWindow, Account assignee)
        {
            var replaceValues = new Dictionary<string, object>
            {
                {"Description", releaseTask.Description},
                {"Type", releaseTask.Type},
                {"Risk", releaseTask.Risk},
                {"HelpDeskUrl", !string.IsNullOrEmpty(releaseTask.HelpDeskTicketReference)
                    ? string.Format("<a href=\"{0}\">#{1}</a>", releaseTask.HelpDeskTicketUrl, releaseTask.HelpDeskTicketReference)
                    : "not created"},
                {"TaskExternalId", releaseTask.ExternalId.ToString()},
                {"Assignee", assignee.FullName},
                {"Products", releaseWindow.Products.FormatElements(string.Empty, string.Empty)},
                {"Sprint", releaseWindow.Sprint},
                {"StartTime", String.Format("{0:dd/MM/yyyy HH:mm}", releaseWindow.StartTime.ToLocalTime())},
                {"ConfirmUrl", string.Format("{0}confirm?{1}={2}&{3}={4}", ConfigurationManager.AppSettings["frontendUrl"], 
                    "releaseTaskUpdateAcknowledge", releaseTask.ExternalId, "releaseWindowId", releaseWindow.ExternalId)},
            };

            var assigneeEmail = EmailTextProvider.GetText("TaskWasUpdatedEmail", replaceValues);

            EmailService.Send(assignee.Email,
                string.Format("{0} Release task updated", releaseWindow.Products.FormatElements(string.Empty, string.Empty)),
                assigneeEmail);
        }

        private void NotifySupportSubscribers(ReleaseTask releaseTask, ReleaseWindow releaseWindow, Account assignee)
        {
            IEnumerable<Account> accounts;
            using (var gateway = AccountNotificationGatewayFactory())
            {
                accounts = gateway.GetSubscribers(NotificationType.ReleaseTasks, releaseWindow.Products)
                    .Where(
                        o =>
                            o.ExternalId != releaseTask.AssigneeExternalId)
                    .ToList();
            }

            foreach (var recipient in accounts)
            {
                var replaceValues = new Dictionary<string, object>
                {
                    {"Recipient", recipient.FullName},
                    {"Description", releaseTask.Description},
                    {"Type", releaseTask.Type},
                    {"Risk", releaseTask.Risk},
                    {"Assignee", assignee.FullName},
                    {"HelpDeskUrl", !string.IsNullOrEmpty(releaseTask.HelpDeskTicketReference)
                        ? string.Format("<a href=\"{0}\">#{1}</a>", releaseTask.HelpDeskTicketUrl, releaseTask.HelpDeskTicketReference)
                        : "not created"},
                    {"TaskExternalId", releaseTask.ExternalId.ToString()},
                    {"Products", releaseWindow.Products.FormatElements(string.Empty, string.Empty)},
                    {"Sprint", releaseWindow.Sprint},
                    {"StartTime", String.Format("{0:dd/MM/yyyy HH:mm}", releaseWindow.StartTime.ToLocalTime())},
                    {"ReleasePlanUrl", string.Format("{0}release?{1}={2}", ConfigurationManager.AppSettings["frontendUrl"], "releaseWindowId", releaseWindow.ExternalId)},
                };

                var assigneeEmail = EmailTextProvider.GetText("TaskWasUpdatedSupportMembersEmail", replaceValues);
                EmailService.Send(recipient.Email,
                    string.Format("{0} Release task updated", releaseWindow.Products.FormatElements(string.Empty, string.Empty)),
                    assigneeEmail);
            }
        }

        private Account GetAccount(Guid accountId)
        {
            Account account;
            using (var gateway = AccountsGatewayFactory())
            {
                account = gateway.GetAccount(accountId, true);
            }

            return account;
        }

        private ReleaseWindow GetReleaseWindow(Guid releaseWindiowId)
        {
            ReleaseWindow releaseWindow;
            using (var gateway = ReleaseWindowGatewayFactory())
            {
                releaseWindow = gateway.GetByExternalId(releaseWindiowId, true);
            }

            return releaseWindow;
        }
    }
}
