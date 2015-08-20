using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using ReMi.BusinessEntities.Products;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.BusinessLogic;
using ReMi.Commands.ReleaseExecution;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Constants.Subscriptions;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Contracts.Plugins.Services.Email;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.Subscriptions;
using ReMi.DataAccess.Exceptions;
using ReMi.Events.ReleaseCalendar;
using ReMi.Queries.ReleasePlan;

namespace ReMi.EventHandlers.ReleaseCalendar
{
    public class ReleaseWindowClosedEventHandler : IHandleEvent<ReleaseWindowClosedEvent>
    {
        public IHandleQuery<GetReleaseContentInformationRequest, GetReleaseContentInformationResponse> GetReleaseContentInformationQuery { get; set; }
        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }
        public Func<IProductGateway> PackageGatewayFactory { get; set; }
        public Func<IAccountNotificationGateway> AccountNotificationGatewayFactory { get; set; }
        public IEmailService EmailService { get; set; }
        public IEmailTextProvider EmailTextProvider { get; set; }
        public ICommandDispatcher CommandDispatcher { get; set; }

        private const string ReleaseClosedSubject = "{0} Release closed";

        public void Handle(ReleaseWindowClosedEvent evnt)
        {
            ReleaseWindow releaseWindow;
            using (var gateway = ReleaseWindowGatewayFactory())
            {
                releaseWindow = gateway.GetByExternalId(evnt.ReleaseWindowId, getReleaseNote: true);
            }

            if (releaseWindow == null)
            {
                throw new ReleaseWindowNotFoundException(evnt.ReleaseWindowId);
            }

            List<string> addressList;
            using (var gateway = AccountNotificationGatewayFactory())
            {
                addressList = gateway.GetSubscribers(NotificationType.Closing, releaseWindow.Products)
                    .Select(x => x.Email).ToList();
            }

            if (evnt.Recipients != null)
            {
                addressList.AddRange(evnt.Recipients.Where(x => !addressList.Contains(x.Email)).Select(a => a.Email));
            }

            if (releaseWindow.ReleaseType == ReleaseType.Scheduled ||
                releaseWindow.ReleaseType == ReleaseType.Automated)
            {
                List<ReleaseContentTicket> includedContent;
                if (!evnt.IsFailed)
                {
                    var content =
                        GetReleaseContentInformationQuery.Handle(new GetReleaseContentInformationRequest
                        {
                            ReleaseWindowId = evnt.ReleaseWindowId
                        });

                    includedContent = content.Content.Where(c => c.IncludeToReleaseNotes).ToList();
                    IEnumerable<Product> packages;
                    using (var gateway = PackageGatewayFactory())
                    {
                        packages = gateway.GetProducts(releaseWindow.ExternalId);
                    }

                    CommandDispatcher.Send(new UpdateTicketLabelsCommand
                    {
                        Tickets = includedContent,
                        PackageId = packages.First().ExternalId
                    });
                }
                else
                {
                    includedContent = new List<ReleaseContentTicket>();
                }

                if (addressList.Any())
                {
                    var replaceValues = new Dictionary<string, object>
                    {
                        {"Sprint", releaseWindow.Sprint},
                        {"Products", releaseWindow.Products.FormatElements(string.Empty, string.Empty)},
                        {"Notes", releaseWindow.ReleaseNotes},
                        {"Issues", releaseWindow.Issues.IsNullOrEmpty() ? "None" : releaseWindow.Issues},
                        {"Tickets", string.Empty},
                        {
                            "ReleasePlanUrl",
                            string.Format("{0}release?{1}={2}", ConfigurationManager.AppSettings["frontendUrl"],
                                "releaseWindowId", releaseWindow.ExternalId)
                        }
                    };


                    var tickets = "Released tickets:<table>";
                    includedContent.ForEach(
                        t =>
                        {
                            tickets += String.Format("<tr><td><a href=\"{2}\">{0}</a></td><td>{1}</td></tr>",
                                t.TicketName, t.TicketDescription, t.TicketUrl);
                        });
                    tickets += "</table>";

                    replaceValues["Tickets"] = tickets;


                    var email = EmailTextProvider.GetText("ReleaseClosedEmail", replaceValues);

                    EmailService.Send(addressList,
                        string.Format(ReleaseClosedSubject, releaseWindow.Products.FormatElements(string.Empty, string.Empty)),
                        email);
                }
            }
        }
    }
}
