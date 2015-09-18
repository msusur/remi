using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessLogic.Auth;
using ReMi.Commands.ReleaseCalendar;
using ReMi.Common.Constants.ReleasePlan;
using ReMi.Common.Utils.Enums;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.Events.ReleaseCalendar;
using ReMi.Events.ReleaseExecution;
using System;
using System.Linq;
using ReMi.Common.Utils;

namespace ReMi.CommandHandlers.ReleaseCalendar
{
    public class FailReleaseHandler : IHandleCommand<FailReleaseCommand>
    {
        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }
        public ICommandDispatcher CommandDispatcher { get; set; }

        public IPublishEvent PublishEvent { get; set; }
        public IAccountsBusinessLogic AccountsBusinessLogic { get; set; }

        public void Handle(FailReleaseCommand command)
        {
            var session = AccountsBusinessLogic.SignSession(command.UserName, command.Password);
            if (session == null)
                throw new FailedToAuthenticateException(command.UserName);

            CommandDispatcher.Send(new ClearReleaseContentCommand
            {
                ReleaseWindowId = command.ReleaseWindowId,
                CommandContext = command.CommandContext.CreateChild()
            });
            CommandDispatcher.Send(new ClearReleaseChangesCommand
            {
                ReleaseWindowId = command.ReleaseWindowId,
                CommandContext = command.CommandContext.CreateChild()
            });

            CloseFailedRelease(command.ReleaseWindowId, command.Issues);
        }

        private void CloseFailedRelease(Guid releaseWindowId, string issues)
        {
            using (var gateway = ReleaseWindowGatewayFactory())
            {
                gateway.CloseFailedRelease(releaseWindowId, issues);
            }

            PublishEvent.Publish(new ReleaseWindowClosedEvent
            {
                Recipients = Enumerable.Empty<Account>(),
                ReleaseWindowId = releaseWindowId,
                IsFailed = true
            });

            PublishEvent.Publish(new ReleaseStatusChangedEvent
            {
                ReleaseWindowId = releaseWindowId,
                ReleaseStatus = EnumDescriptionHelper.GetDescription(ReleaseStatus.Closed)
            });
        }
    }
}
