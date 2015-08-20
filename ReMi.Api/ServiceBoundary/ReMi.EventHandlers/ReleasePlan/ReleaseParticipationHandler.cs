using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Common.Logging;
using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.BusinessLogic;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Plugins.Data.Email;
using ReMi.Contracts.Plugins.Services.Email;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.Events.ReleasePlan;

namespace ReMi.EventHandlers.ReleasePlan
{
    public class ReleaseParticipationHandler :
        IHandleEvent<ReleaseParticipantsAddedEvent>,
        IHandleEvent<ReleaseParticipantRemovedEvent>
    {
        public IEmailTextProvider EmailTextProvider { get; set; }
        public IEmailService EmailService { get; set; }
        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }
        public IApplicationSettings ApplicationSettings { get; set; }

        private readonly ILog _log = LogManager.GetCurrentClassLogger();
        private const string EmailSubject = "{0} Release Support Team";

        public void Handle(ReleaseParticipantsAddedEvent evnt)
        {
            if (evnt.Participants == null || !evnt.Participants.Any())
            {
                _log.InfoFormat("No people were defined to send email to");
                return;
            }

            var release = GetReleaseWindow(evnt.ReleaseWindowId);

            foreach (var participant in evnt.Participants)
            {
                var replaceValues = GetEmailParameters(release, participant);

                replaceValues.Add(
                    "AcknowledgeUrl",
                    string.Format("{0}={1}", ConfigurationManager.AppSettings["acknowledgeUrl"],
                        participant.ReleaseParticipantId));

                var email = EmailTextProvider.GetText("ParticipantAddedToReleaseWindowEmail", replaceValues);
                
                _log.DebugFormat("Sending email: {0}", email);

                EmailService.SendWithCalendarEvent(
                    participant.Account.Email,
                    string.Format(EmailSubject, release.Products.FormatElements(string.Empty, string.Empty)),
                    email,
                    CreateCalendarEntity(release, CalendarEventType.REQUEST),
                    ApplicationSettings.DefaultReleaseWindowDurationTime);
            }
        }

        public void Handle(ReleaseParticipantRemovedEvent evnt)
        {
            var release = GetReleaseWindow(evnt.ReleaseWindowId);

            var replaceValues = GetEmailParameters(release, evnt.Participant);

            replaceValues.Add(
                "ReleasePlanUrl",
                string.Format("{0}release?{1}={2}", ConfigurationManager.AppSettings["frontendUrl"], "releaseWindowId",
                    release.ExternalId));
         
            var email = EmailTextProvider.GetText("ParticipantRemovedFromReleaseWindowEmail", replaceValues);

            _log.DebugFormat("Sending email: {0}", email);

            EmailService.SendWithCalendarEvent(evnt.Participant.Account.Email,
                string.Format(EmailSubject, release.Products.FormatElements(string.Empty, string.Empty)),
                email,
                CreateCalendarEntity(release, CalendarEventType.CANCEL),
                ApplicationSettings.DefaultReleaseWindowDurationTime);
        }

        private ReleaseWindow GetReleaseWindow(Guid windowId)
        {
            using (var gateway = ReleaseWindowGatewayFactory())
            {
                var release = gateway.GetByExternalId(windowId);
                if (release == null)
                {
                    throw new ReleaseNotFoundException(windowId);
                }

                return release;
            }
        }

        private Dictionary<string, object> GetEmailParameters(ReleaseWindow releaseWindow,
            ReleaseParticipant participant)
        {
            return new Dictionary<string, object>
            {
                {"Participant", participant.Account.FullName},
                {"Sprint", releaseWindow.Sprint},
                {"Products", releaseWindow.Products.FormatElements(string.Empty, string.Empty)},
                {"StartTime", string.Format("{0:dd/MM/yyyy HH:mm}", releaseWindow.StartTime.ToLocalTime())}
            };
        }

        private CalendarEvent CreateCalendarEntity(ReleaseWindow release, CalendarEventType method)
        {
            return new CalendarEvent
            {
                AppointmentId = release.ExternalId.ToString(),
                Location = string.Empty,
                CalendarEventType = method,
                StartTime = release.StartTime
            };
        }
    }
}
