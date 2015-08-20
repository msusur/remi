using System;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Commands.ReleasePlan;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Events.ReleasePlan;

namespace ReMi.CommandHandlers.ReleasePlan
{
    public class CreateReleaseTaskCommandHandler : IHandleCommand<CreateReleaseTaskCommand>
    {
        public Func<IAccountsGateway> AccountGatewayFactory { get; set; }
        public Func<IReleaseTaskGateway> ReleaseTaskGatewayFactory { get; set; }
        public ICommandDispatcher CommandDispatcher { get; set; }
        public IPublishEvent PublishEvent { get; set; }

        public void Handle(CreateReleaseTaskCommand command)
        {
            var task = command.ReleaseTask;

            task.CreatedBy = command.CommandContext.UserName;
            task.CreatedByExternalId = command.CommandContext.UserId;

            CreateReleaseTask(task);
        }

        private void CreateReleaseTask(ReleaseTask releaseTask)
        {
            using (var gateway = AccountGatewayFactory())
            {
                var assignee = gateway.GetAccount(releaseTask.AssigneeExternalId, true);

                gateway.AssociateAccountsWithProduct(new[] { assignee.Email }, releaseTask.ReleaseWindowId);
            }

            using (var gateway = ReleaseTaskGatewayFactory())
            {
                gateway.CreateReleaseTask(releaseTask);
            }
        
            PublishEvent.Publish(new ReleaseTaskCreatedEvent { ReleaseTask = releaseTask });

            if (releaseTask.CreateHelpDeskTicket)
                CommandDispatcher.Send(new CreateHelpDeskTaskCommand { ReleaseTask = releaseTask });

            //CommandDispatcher.Send(new AttachFileToReleaseTaskCommand { ReleaseTask = releaseTask });
        }
    }
}
