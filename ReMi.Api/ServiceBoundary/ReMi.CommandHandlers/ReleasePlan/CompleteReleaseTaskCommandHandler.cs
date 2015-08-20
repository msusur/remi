using System;
using ReMi.Commands.ReleasePlan;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Events.ReleasePlan;

namespace ReMi.CommandHandlers.ReleasePlan
{
    public class CompleteReleaseTaskCommandHandler : IHandleCommand<CompleteReleaseTaskCommand>
    {
        public Func<IReleaseTaskGateway> ReleaseTaskGatewayFactory { get; set; }
        public IPublishEvent EventPublisher { get; set; }

        public void Handle(CompleteReleaseTaskCommand command)
        {
            using (var gateway = ReleaseTaskGatewayFactory())
            {
                gateway.CompleteTask(command.ReleaseTaskExtetnalId, command.CommandContext.UserId);
                var task = gateway.GetReleaseTask(command.ReleaseTaskExtetnalId);

                EventPublisher.Publish(new TaskCompletedEvent
                {
                    ReleaseTaskExternalId = command.ReleaseTaskExtetnalId,
                    ReleaseWindowExternalId = task.ReleaseWindowId,
                    AssigneeName = task.Assignee
                });
            }


        }
    }
}
