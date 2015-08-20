using System;
using ReMi.Commands.Metrics;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Metrics;
using ReMi.Events.Metrics;

namespace ReMi.CommandHandlers.Metrics
{
    public class UpdateMetricsCommandHandler : IHandleCommand<UpdateMetricsCommand>
    {
        public Func<IMetricsGateway> MetricsGatewayFactory { get; set; }
        public IPublishEvent EventPublisher { get; set; }

        public void Handle(UpdateMetricsCommand command)
        {
            command.Metric.ExecutedOn = SystemTime.Now;

            using (var gateway = MetricsGatewayFactory())
            {
                gateway.UpdateMetrics(command.Metric);
            }

            command.Metric.ExecutedOn = command.Metric.ExecutedOn.Value.ToLocalTime();
            EventPublisher.Publish(new MetricsUpdatedEvent
            {
                Metric = command.Metric,
                ReleaseWindowId = command.ReleaseWindowId
            });
        }

       
    }
}
