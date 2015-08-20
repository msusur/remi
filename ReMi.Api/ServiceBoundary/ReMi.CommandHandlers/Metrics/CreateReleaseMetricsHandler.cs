using System;
using System.Collections.Generic;
using ReMi.BusinessLogic.ReleasePlan;
using ReMi.Commands.Metrics;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Constants.ReleaseExecution;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.Metrics;

namespace ReMi.CommandHandlers.Metrics
{
    public class CreateReleaseMetricsHandler : IHandleCommand<CreateReleaseMetricsCommand>
    {
        public Func<IMetricsGateway> MetricsGatewayFactory { get; set; }
        public IReleaseWindowHelper ReleaseWindowHelper { get; set; }

        public void Handle(CreateReleaseMetricsCommand command)
        {
            using (var gateway = MetricsGatewayFactory())
            {
                gateway.CreateMetrics(
                    command.ReleaseWindow.ExternalId,
                    new Dictionary<MetricType, DateTime?> {
                        { MetricType.Approve, null },
                        { MetricType.SignOff, null },
                        { MetricType.Close, null },
                        { MetricType.StartTime, command.ReleaseWindow.StartTime },
                        { MetricType.EndTime, command.ReleaseWindow.EndTime },
                    });

                if (ReleaseWindowHelper.IsMaintenance(command.ReleaseWindow))
                {
                    gateway.CreateMetrics(command.ReleaseWindow.ExternalId,
                            new[] { MetricType.StartRun, MetricType.FinishRun });
                }
                else
                    switch (command.ReleaseWindow.ReleaseType)
                    {
                        case ReleaseType.Automated:
                            gateway.CreateMetrics(command.ReleaseWindow.ExternalId,
                                new[] { MetricType.StartDeploy, MetricType.FinishDeploy });

                            break;
                        case ReleaseType.Scheduled:
                            gateway.CreateMetrics(command.ReleaseWindow.ExternalId,
                                new[] { MetricType.StartDeploy, MetricType.FinishDeploy, MetricType.Complete });
                            break;
                        case ReleaseType.ChangeRequest:
                        case ReleaseType.Hotfix:
                            gateway.CreateMetrics(command.ReleaseWindow.ExternalId,
                                new[] { MetricType.StartRun, MetricType.FinishRun, MetricType.Complete });
                            break;
                    }

                if (command.ReleaseWindow.RequiresDowntime)
                {
                    gateway.CreateMetrics(command.ReleaseWindow.ExternalId,
                        new[] { MetricType.SiteDown, MetricType.SiteUp });
                }
            }
        }
    }
}
