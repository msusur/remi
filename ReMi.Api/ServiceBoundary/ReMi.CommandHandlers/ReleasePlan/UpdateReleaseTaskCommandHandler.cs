using System;
using ReMi.Commands.ReleasePlan;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Events.ReleasePlan;

namespace ReMi.CommandHandlers.ReleasePlan
{
    public class UpdateReleaseTaskCommandHandler : IHandleCommand<UpdateReleaseTaskCommand>
    {
        public Func<IReleaseTaskGateway> ReleaseTaskGatewayFactory { get; set; }
        public ICommandDispatcher CommandDispatcher { get; set; }
        public IPublishEvent PublishEvent { get; set; }

        public void Handle(UpdateReleaseTaskCommand command)
        {
            using (var gateway = ReleaseTaskGatewayFactory())
            {
                var isFieldsUpdated = gateway.UpdateReleaseTask(command.ReleaseTask);

                var releaseTask = gateway.GetReleaseTask(command.ReleaseTask.ExternalId);

                if (isFieldsUpdated)
                {
                    PublishEvent.Publish(new ReleaseTaskUpdatedEvent { ReleaseTask = releaseTask });

                    if (!string.IsNullOrEmpty(releaseTask.HelpDeskTicketReference))
                        CommandDispatcher.Send(new UpdateHelpDeskTaskCommand { ReleaseTask = releaseTask });

                    CommandDispatcher.Send(new CleanReleaseTaskReceiptCommand { ReleaseTaskId = releaseTask.ExternalId });
                }

                if (string.IsNullOrEmpty(releaseTask.HelpDeskTicketReference) && command.ReleaseTask.CreateHelpDeskTicket)
                    CommandDispatcher.Send(new CreateHelpDeskTaskCommand { ReleaseTask = releaseTask });

                //CommandDispatcher.Send(new AttachFileToReleaseTaskCommand { ReleaseTask = releaseTask });
            }
        }
    }
}
