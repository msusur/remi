using System;
using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessLogic.Auth;
using ReMi.BusinessLogic.ReleasePlan;
using ReMi.Commands.ReleaseCalendar;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.Auth;

namespace ReMi.CommandHandlers.ReleasePlan
{
    public class CloseReleaseHandler : IHandleCommand<CloseReleaseCommand>
    {
        public Func<IAccountsGateway> AccountsGatewayFactory { get; set; }
        public IReleaseWindowStateUpdater ReleaseWindowStateUpdater { get; set; }
        public IAccountsBusinessLogic AccountsBusinessLogic { get; set; }

        public void Handle(CloseReleaseCommand command)
        {
            var session = AccountsBusinessLogic.SignSession(command.UserName, command.Password);
            if (session == null)
                throw new FailedToAuthenticateException(command.UserName);
            if (command.CommandContext.UserId == Guid.Empty
                || session.AccountId != command.CommandContext.UserId)
                throw new AttemptToSignAsOtherAccountException(command.UserName, command.CommandContext.UserId);

            ReleaseWindowStateUpdater.CloseRelease(command.ReleaseWindowId, command.ReleaseNotes, command.Recipients, command.CommandContext.UserId);
        }
    }
}
