using System;
using System.Linq;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessLogic.Auth;
using ReMi.Commands.ReleaseCalendar;
using ReMi.Common.Constants.ReleasePlan;
using ReMi.Common.Utils.Enums;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.DataAccess.BusinessEntityGateways.SourceControl;
using ReMi.Events.ReleaseCalendar;
using ReMi.Events.ReleaseExecution;

namespace ReMi.CommandHandlers.ReleaseCalendar
{
    public class FailReleaseHandler : IHandleCommand<FailReleaseCommand>
    {
        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }
        public Func<IReleaseContentGateway> ReleaseContentGatewayFactory { get; set; }
        public Func<ISourceControlChangeGateway> SourceControlChangesGatewayFactory { get; set; }

        public IPublishEvent PublishEvent { get; set; }
        public IAccountsBusinessLogic AccountsBusinessLogic { get; set; }

        public void Handle(FailReleaseCommand command)
        {
            var session = AccountsBusinessLogic.SignSession(command.UserName, command.Password);
            if (session == null)
                throw new FailedToAuthenticateException(command.UserName);

            RemoveTickets(command.ReleaseWindowId);

            RemoveChanges(command.ReleaseWindowId);

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

        private void RemoveTickets(Guid releaseWindowId)
        {
            using (var gateway = ReleaseContentGatewayFactory())
            {
                gateway.RemoveTicketsFromRelease(releaseWindowId);
            }
        }

        private void RemoveChanges(Guid releaseWindowId)
        {
            using (var gateway = SourceControlChangesGatewayFactory())
            {
                gateway.RemoveChangesFromRelease(releaseWindowId);
            }
        }

    }
}
