using System;
using System.Linq;
using ReMi.Commands.ReleasePlan;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Events.ReleasePlan;

namespace ReMi.CommandHandlers.ReleasePlan
{
    public class UpdateReleaseTasksOrderCommandHandler : IHandleCommand<UpdateReleaseTasksOrderCommand>
    {
        public Func<IReleaseTaskGateway> ReleaseTaskGatewayFactory { get; set; }
        public IPublishEvent PublishEvent { get; set; }
 
        public void Handle(UpdateReleaseTasksOrderCommand command)
        {
            Guid releaseWindowId;
            using (var gateway = ReleaseTaskGatewayFactory())
            {
                gateway.UpdateReleaseTasksOrder(command.ReleaseTasksOrder);
                releaseWindowId = gateway.GetReleaseTask(command.ReleaseTasksOrder.First().Key).ReleaseWindowId;
            }

            PublishEvent.Publish(new ReleaseTasksOrderUpdatedEvent
            {
                ReleaseWindowId = releaseWindowId
            });
        }
    }
}
