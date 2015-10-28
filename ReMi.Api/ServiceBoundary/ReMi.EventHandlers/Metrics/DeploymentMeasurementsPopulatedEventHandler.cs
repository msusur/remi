using ReMi.Commands.Metrics;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Events.Metrics;

namespace ReMi.EventHandlers.Metrics
{
    public class DeploymentMeasurementsPopulatedEventHandler : IHandleEvent<DeploymentMeasurementsPopulatedEvent>
    {
        public ICommandDispatcher CommandDispatcher { get; set; }

        public void Handle(DeploymentMeasurementsPopulatedEvent evnt)
        {
            CommandDispatcher.Send(new CalculateDeployTimeCommand
            {
                CommandContext = evnt.Context.CreateChild<CommandContext>(),
                ReleaseWindowId = evnt.ReleaseWindowId
            });
        }
    }
}
