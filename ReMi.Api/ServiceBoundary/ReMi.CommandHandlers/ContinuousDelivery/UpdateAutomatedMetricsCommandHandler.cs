using System;
using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessEntities.Metrics;
using ReMi.Commands.ContinuousDelivery;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Metrics;
using ReMi.Events.Metrics;

namespace ReMi.CommandHandlers.ContinuousDelivery
{
    public class UpdateAutomatedMetricsCommandHandler : IHandleCommand<UpdateAutomatedMetricsCommand>
    {
        public Func<IMetricsGateway> MetricsGatewayFactory { get; set; }
        public IPublishEvent EventPublisher { get; set; }

        public void Handle(UpdateAutomatedMetricsCommand command)
        {
            Metric metric;
            using (var gateway = MetricsGatewayFactory())
            {
                metric = gateway.GetMetric(command.ReleaseWindowId, command.MetricType);
                if (metric == null)
                {
                    throw new MissingMetricException(command.ReleaseWindowId, command.MetricType);
                }
                metric.ExecutedOn = SystemTime.Now;
                gateway.UpdateMetrics(metric);
            }

            metric.ExecutedOn = metric.ExecutedOn.Value.ToLocalTime();
            EventPublisher.Publish(new MetricsUpdatedEvent
            {
                Metric = metric,
                ReleaseWindowId = command.ReleaseWindowId
            });
        }

       
    }
}
