using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.BusinessLogic.BusinessRules;
using ReMi.Commands.ReleasePlan;
using ReMi.Common.Constants.BusinessRules;
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
        public IBusinessRuleEngine BusinessRuleEngine { get; set; }

        public void Handle(CreateReleaseTaskCommand command)
        {
            var task = command.ReleaseTask;

            task.CreatedBy = command.CommandContext.UserName;
            task.CreatedByExternalId = command.CommandContext.UserId;

            CreateReleaseTask(task, command.CommandContext.UserId);
        }

        private void CreateReleaseTask(ReleaseTask releaseTask, Guid userId)
        {
            using (var gateway = AccountGatewayFactory())
            {
                var assignee = gateway.GetAccount(releaseTask.AssigneeExternalId, true);

                gateway.AssociateAccountsWithProduct(new[] { assignee.Email }, releaseTask.ReleaseWindowId, GetRule(userId));
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

        private Func<string, TeamRoleRuleResult> GetRule(Guid userId)
        {
            return s => BusinessRuleEngine.Execute<TeamRoleRuleResult>(
                userId, BusinessRuleConstants.Config.TeamRoleRule.ExternalId,
                new Dictionary<string, object> { { "roleName", s } });
        }
    }
}
