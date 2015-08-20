using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessLogic;
using ReMi.Common.Constants.Subscriptions;
using ReMi.Common.Utils;
using ReMi.Common.Utils.Enums;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Plugins.Data.Email;
using ReMi.Contracts.Plugins.Services.Email;
using ReMi.DataAccess.BusinessEntityGateways.Subscriptions;
using ReMi.Events.ReleaseCalendar;

namespace ReMi.EventHandlers.ReleaseCalendar
{
    public class ReleaseWindowCanceledSendNotificationHandler : IHandleEvent<ReleaseWindowCanceledEvent>
    {
        public Func<IAccountNotificationGateway> AccountNotificationGatewayFactory { get; set; }
        public IEmailService EmailService { get; set; }
        public IEmailTextProvider EmailTextProvider { get; set; }
        public IApplicationSettings ApplicationSettings { get; set; }

        private const string EmailSupportTeamSubject = "{0} Release Support Team";

        public void Handle(ReleaseWindowCanceledEvent evnt)
        {
            if (evnt.ReleaseWindow == null)
                throw new ArgumentException("ReleaseWindow are not initialized");

            foreach (var account in evnt.Participants)
            {
                var email = CreateEmail(account, evnt.ReleaseWindow);

                EmailService.SendWithCalendarEvent(account.Email,
                    string.Format(EmailSupportTeamSubject, evnt.ReleaseWindow.Products.FormatElements(string.Empty, string.Empty)),
                    email,
                    new CalendarEvent
                    {
                        AppointmentId = evnt.ReleaseWindow.ExternalId.ToString(),
                        Location = string.Empty,
                        CalendarEventType = CalendarEventType.CANCEL,
                        StartTime = evnt.ReleaseWindow.StartTime
                    },
                    ApplicationSettings.DefaultReleaseWindowDurationTime);
            }

            using (var gateway = AccountNotificationGatewayFactory())
            {
                var subscribers = gateway.GetSubscribers(NotificationType.ReleaseWindowsSchedule,
                    evnt.ReleaseWindow.Products)
                    .Where(
                        x => evnt.Participants.All(e => e.ExternalId != x.ExternalId));

                foreach (var subscriber in subscribers)
                {
                    var email = CreateEmail(subscriber, evnt.ReleaseWindow);

                    EmailService.Send(subscriber.Email,
                        string.Format(EmailSupportTeamSubject, evnt.ReleaseWindow.Products.FormatElements(string.Empty, string.Empty)),
                        email);
                }
            }
        }

        private string CreateEmail(Account account, ReleaseWindow releaseWindow)
        {
            var replaceValues = new Dictionary<string, object>
                {
                    {"Assignee", account.FullName},
                    {"Products", releaseWindow.Products.FormatElements(string.Empty, string.Empty)},
                    {"ReleaseType", EnumDescriptionHelper.GetDescription(releaseWindow.ReleaseType) },
                    {"Sprint", releaseWindow.Sprint},
                    {"StartTime", string.Format("{0:dd/MM/yyyy HH:mm}", releaseWindow.StartTime.ToLocalTime())},
                    {"ReleaseCalendarUrl", ConfigurationManager.AppSettings["frontendUrl"]},
                };

            return EmailTextProvider.GetText("ReleaseWindowCancelled", replaceValues);
        }
    }
}
