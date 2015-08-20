using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Common.Logging;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.BusinessLogic;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Plugins.Data.Email;
using ReMi.Contracts.Plugins.Services.Email;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Events.ReleaseCalendar;

namespace ReMi.EventHandlers.ReleaseCalendar
{
    public class ReleaseWindowUpdatedSendsNotificationsToParticipatnsHandler : IHandleEvent<ReleaseWindowUpdatedEvent>
    {
        public Func<IReleaseParticipantGateway> ReleaseParticipantGatewayFactory { get; set; }
        public IEmailService EmailService { get; set; }
        public IApplicationSettings ApplicationSettings { get; set; }
        public IEmailTextProvider EmailTextProvider { get; set; }

        private readonly ILog _log = LogManager.GetCurrentClassLogger();
        private const string EmailSubject = "{0} Release Support Team";

        public void Handle(ReleaseWindowUpdatedEvent evnt)
        {
            if (evnt.ReleaseWindow == null)
                throw new InvalidDataException("ReleaseWindow is not initialized");

            List<ReleaseParticipant> participants;
            using (var gateway = ReleaseParticipantGatewayFactory())
            {
                participants = gateway.GetReleaseParticipants(evnt.ReleaseWindow.ExternalId).ToList();
            }

            foreach (var participant in participants)
            {
                var email = EmailTextProvider.GetText("ReleaseWindowUpdatedParticipantEmail",
                    new Dictionary<string, object>
                    {
                        {"Assignee", participant.Account.FullName},
                        {"Products", evnt.ReleaseWindow.Products.FormatElements(string.Empty, string.Empty)},
                        {"Sprint", evnt.ReleaseWindow.Sprint},
                        {"StartTime", string.Format("{0:dd/MM/yyyy HH:mm}", evnt.ReleaseWindow.StartTime)},
                        {
                            "ConfirmationUrl",
                            string.Format("{0}={1}", ConfigurationManager.AppSettings["acknowledgeUrl"],
                                participant.ReleaseParticipantId)
                        }
                    });

                _log.DebugFormat("Sending email: {0}", email);

                EmailService.SendWithCalendarEvent(participant.Account.Email,
                    string.Format(EmailSubject, evnt.ReleaseWindow.Products.FormatElements(string.Empty, string.Empty)),
                    email,
                    new CalendarEvent
                    {
                        AppointmentId = evnt.ReleaseWindow.ExternalId.ToString(),
                        Location = string.Empty,
                        CalendarEventType = CalendarEventType.REQUEST,
                        StartTime = evnt.ReleaseWindow.StartTime
                    },
                    ApplicationSettings.DefaultReleaseWindowDurationTime);
            }
        }
    }
}

