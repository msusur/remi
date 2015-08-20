using System;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Commands.ReleasePlan;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Events.ReleasePlan;

namespace ReMi.CommandHandlers.ReleasePlan
{
    public class DeleteReleaseTaskCommandHandler : IHandleCommand<DeleteReleaseTaskCommand>
    {
        public Func<IReleaseTaskGateway> ReleaseTaskGatewayFactory { get; set; }
        public IPublishEvent PublishEvent { get; set; }
        public ICommandDispatcher CommandDispatcher { get; set; }

        public void Handle(DeleteReleaseTaskCommand command)
        {
            ReleaseTask releaseTask;
            using (var gateway = ReleaseTaskGatewayFactory())
            {
                releaseTask = gateway.GetReleaseTask(command.ReleaseTaskId);

                if (releaseTask.CompletedOn.HasValue)
                {
                    throw new Exception("Can't remove completed task");
                }

                gateway.DeleteReleaseTask(command.ReleaseTaskId);
            }

            if (!string.IsNullOrEmpty(releaseTask.HelpDeskTicketReference))
            {
                CommandDispatcher.Send(new DeleteHelpDeskTaskCommand
                {
                    HelpDeskTicketRef = releaseTask.HelpDeskTicketReference,
                    ReleaseWindowId = releaseTask.ReleaseWindowId
                });
            }
            PublishEvent.Publish(new ReleaseTaskDeletedEvent { ReleaseTask = releaseTask });
        }
    }
}
