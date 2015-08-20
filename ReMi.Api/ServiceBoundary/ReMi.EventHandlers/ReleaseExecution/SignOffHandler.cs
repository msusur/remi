using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessEntities.ReleaseExecution;
using ReMi.BusinessLogic;
using ReMi.Common.Constants.Subscriptions;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Plugins.Services.Email;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseExecution;
using ReMi.DataAccess.BusinessEntityGateways.Subscriptions;
using ReMi.DataAccess.Exceptions;
using ReMi.Events.ReleaseExecution;

namespace ReMi.EventHandlers.ReleaseExecution
{
    public class SignOffHandler : IHandleEvent<ReleaseSignersAddedEvent>, IHandleEvent<RemoveSignOffEvent>, IHandleEvent<ReleaseWindowSignedOffEvent>
    {
        public IEmailTextProvider EmailTextProvider { get; set; }
        public IEmailService EmailService { get; set; }
        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }
        public Func<ISignOffGateway> SignOffGatewayFactory { get; set; }
        public Func<IAccountNotificationGateway> AccountNotificationGatewayFactory { get; set; }

        private const string AddedToSignOffsSubject = "{0} Added to sign offs";
        private const string ReleaseSignedOffSubject = "{0} Release signed off";

        public void Handle(ReleaseSignersAddedEvent evnt)
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
                var subscribers = notificationGateway.GetSubscribers(NotificationType.Signing, releaseWindow.Products)
                    .Where(x => evnt.SignOffs.Any(s => s.Signer.Email.ToLower() == x.Email.ToLower())).ToList();

                foreach (var signOff in subscribers)
                {
                    var replaceValues = new Dictionary<string, object>
                    {
                        {"Assignee", signOff.FullName},
                        {"Products", releaseWindow.Products.FormatElements(string.Empty, string.Empty)},
                        {"Sprint", releaseWindow.Sprint},
                        {"StartTime", string.Format("{0:dd/MM/yyyy HH:mm}", releaseWindow.StartTime.ToLocalTime())},
                        {
                            "ReleasePlanUrl",
                            string.Format("{0}release?{1}={2}", ConfigurationManager.AppSettings["frontendUrl"],
                                "releaseWindowId", releaseWindow.ExternalId)
                        },
                    };

                    var email = EmailTextProvider.GetText("SignOffAddedToReleaseWindowEmail", replaceValues);

                    EmailService.Send(signOff.Email,
                        string.Format(AddedToSignOffsSubject, releaseWindow.Products.FormatElements(string.Empty, string.Empty)),
                        email);
                }
            }
        }

        public void Handle(RemoveSignOffEvent evnt)
        {
            ReleaseWindow releaseWindow;
            using (var gateway = ReleaseWindowGatewayFactory())
            {
                releaseWindow = gateway.GetByExternalId(evnt.ReleaseWindowGuid);
                if (releaseWindow == null)
                    throw new ReleaseWindowNotFoundException(evnt.ReleaseWindowGuid);
            }

            Account account;
            using (var gateway = AccountNotificationGatewayFactory())
            {
                account = gateway.GetSubscribers(NotificationType.Signing, releaseWindow.Products)
                    .FirstOrDefault(x => x.ExternalId == evnt.AccountId);
            }

            if (account != null)
            {
                var replaceValues = new Dictionary<string, object>
                {
                    {"Assignee", account.FullName},
                    {"Products", releaseWindow.Products.FormatElements(string.Empty, string.Empty)},
                    {"Sprint", releaseWindow.Sprint},
                    {"StartTime", string.Format("{0:dd/MM/yyyy HH:mm}", releaseWindow.StartTime.ToLocalTime())},
                    {
                        "ReleasePlanUrl",
                        string.Format("{0}release?{1}={2}", ConfigurationManager.AppSettings["frontendUrl"],
                            "releaseWindowId", releaseWindow.ExternalId)
                    },
                };

                var email = EmailTextProvider.GetText("SignOffRemovedFromReleaseWindowEmail", replaceValues);

                EmailService.Send(account.Email,
                    string.Format("{0} Removed from signed offs", releaseWindow.Products.FormatElements(string.Empty, string.Empty)),
                    email);
            }
        }

        public void Handle(ReleaseWindowSignedOffEvent evnt)
        {
            IEnumerable<Account> subscribers;
            using (var notificationGateway = AccountNotificationGatewayFactory())
            {
                subscribers = notificationGateway.GetSubscribers(NotificationType.Signing, evnt.ReleaseWindow.Products);
            }

            List<SignOff> signers;
            using (var gateway = SignOffGatewayFactory())
            {
                signers = gateway.GetSignOffs(evnt.ReleaseWindow.ExternalId);
            }

            SendSignOffEmail(evnt.ReleaseWindow, subscribers, signers);
        }

        private void SendSignOffEmail(ReleaseWindow releaseWindow, IEnumerable<Account> adressees, IEnumerable<SignOff> signers)
        {
            var replaceValues = new Dictionary<string, object>
            {
                {"Products", releaseWindow.Products.FormatElements(string.Empty, string.Empty)},
                {"Sprint", releaseWindow.Sprint},
                {
                    "ListOfSignOffs",
                    string.Format("<table style='border: none'><tr><td style=\"padding-right:200px;\">Signer</td><td>Sign off criteria</td></tr><tr><td>{0}</td></tr></table>",
                        string.Join("</td></tr><tr><td>",
                            signers.Select(x => x.Signer.FullName + "</td><td>" + x.Comment).ToList()))
                },
                {"StartTime", string.Format("{0:dd/MM/yyyy HH:mm}", releaseWindow.StartTime.ToLocalTime())},
                {
                    "ReleasePlanUrl",
                    string.Format("{0}release?{1}={2}", ConfigurationManager.AppSettings["frontendUrl"],
                        "releaseWindowId", releaseWindow.ExternalId)
                },
            };

            var email = EmailTextProvider.GetText("ReleaseWindowFullySignedOffEmail", replaceValues);

            EmailService.Send(adressees.Select(s => s.Email).ToList(), 
                string.Format(ReleaseSignedOffSubject, releaseWindow.Products.FormatElements(string.Empty, string.Empty)),
                email);
        }
    }
}
