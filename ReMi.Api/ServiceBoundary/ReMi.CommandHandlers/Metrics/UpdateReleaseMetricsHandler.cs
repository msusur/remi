using System;
using ReMi.Commands.Metrics;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.Metrics;

namespace ReMi.CommandHandlers.Metrics
{
    public class UpdateReleaseMetricsHandler : IHandleCommand<UpdateReleaseMetricsCommand>
    {
        public Func<IMetricsGateway> MetricsGatewayFactory { get; set; }
        public ICommandDispatcher CommandDispatcher { get; set; }

        public void Handle(UpdateReleaseMetricsCommand command)
        {
            using (var gateway = MetricsGatewayFactory())
            {
                gateway.DeleteMetrics(command.ReleaseWindow.ExternalId);
            }

            CommandDispatcher.Send(new CreateReleaseMetricsCommand {ReleaseWindow = command.ReleaseWindow});
        }
    }
}
