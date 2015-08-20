using ReMi.Common.Constants.ReleaseExecution;
using ReMi.Events.Metrics;

namespace ReMi.BusinessLogic.ReleaseExecution
{
    public interface IMetricsChangeEmailNotificationSender
    {
        void Send(MetricsUpdatedEvent evnt, MetricType metric, string emailSubjectTemplate);
    }
}
