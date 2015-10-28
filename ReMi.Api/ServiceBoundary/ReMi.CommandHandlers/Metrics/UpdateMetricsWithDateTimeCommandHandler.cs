using ReMi.BusinessEntities.Metrics;
using ReMi.Commands.Metrics;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Metrics;
using ReMi.Events.Metrics;
using System;

namespace ReMi.CommandHandlers.Metrics
{
    public class UpdateMetricsWithDateTimeCommandHandler : IHandleCommand<UpdateMetricsWithDateTimeCommand>
    {
        public Func<IMetricsGateway> MetricsGatewayFactory { get; set; }
        public IPublishEvent EventPublisher { get; set; }

        public void Handle(UpdateMetricsWithDateTimeCommand command)
        {
            Metric metric;
            using (var gateway = MetricsGatewayFactory())
            {
                gateway.CreateOrUpdateMetric(command.ReleaseWindowId, command.MetricType, command.ExecutedOn);
                metric = gateway.GetMetric(command.ReleaseWindowId, command.MetricType);
            }

            EventPublisher.Publish(new MetricsUpdatedEvent
            {
                Metric = metric,
                ReleaseWindowId = command.ReleaseWindowId,
                Context = command.CommandContext.CreateChild<EventContext>()
            });
        }

       
    }
}
