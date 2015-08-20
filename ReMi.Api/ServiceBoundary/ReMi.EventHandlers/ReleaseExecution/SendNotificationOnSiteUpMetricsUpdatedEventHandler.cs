using ReMi.BusinessLogic.ReleaseExecution;
using ReMi.Common.Constants.ReleaseExecution;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Events.Metrics;

namespace ReMi.EventHandlers.ReleaseExecution
{
    public class SendNotificationOnSiteUpMetricsUpdatedEventHandler : IHandleEvent<MetricsUpdatedEvent>
    {
        public IMetricsChangeEmailNotificationSender EmailSender { get; set; }

        public void Handle(MetricsUpdatedEvent evnt)
        {
            const MetricType metric = MetricType.SiteUp;
            const string emailSubjectTemplate = "{0} MAINTENANCE MODE OFF (SITE ONLINE)";

            EmailSender.Send(evnt, metric, emailSubjectTemplate);
        }

    }
}
