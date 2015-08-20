using ReMi.Commands.DeploymentTool;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseExecution;
using System;

namespace ReMi.CommandHandlers.DeploymentTool
{
    public class RePopulateDeploymentMeasurementsCommandHandler : IHandleCommand<RePopulateDeploymentMeasurementsCommand>
    {
        public Func<IReleaseDeploymentMeasurementGateway> ReleaseDeploymentMeasurementGatewayFactory { get; set; }
        public ICommandDispatcher CommandDispatcher { get; set; }

        public void Handle(RePopulateDeploymentMeasurementsCommand command)
        {
            using (var gateway = ReleaseDeploymentMeasurementGatewayFactory())
            {
                gateway.RemoveDeploymentMeasurements(command.ReleaseWindowId);
            }

            var task = CommandDispatcher.Send(new PopulateDeploymentMeasurementsCommand
            {
                CommandContext = command.CommandContext.CreateChild(),
                ReleaseWindowId = command.ReleaseWindowId
            });
            if (task != null) task.Wait();
        }
    }


}
