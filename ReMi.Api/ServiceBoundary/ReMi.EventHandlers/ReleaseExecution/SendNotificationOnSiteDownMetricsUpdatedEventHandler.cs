using ReMi.BusinessLogic.ReleaseExecution;
using ReMi.Common.Constants.ReleaseExecution;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Events.Metrics;

namespace ReMi.EventHandlers.ReleaseExecution
{
    public class SendNotificationOnSiteDownMetricsUpdatedEventHandler : IHandleEvent<MetricsUpdatedEvent>
    {
        public IMetricsChangeEmailNotificationSender EmailSender { get; set; }

        public void Handle(MetricsUpdatedEvent evnt)
        {
            const MetricType metric = MetricType.SiteDown;
            const string emailSubjectTemplate = "{0} MAINTENANCE MODE ON (SITE OFFLINE)";

            EmailSender.Send(evnt, metric, emailSubjectTemplate);
        }
    }
}
