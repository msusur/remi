using System;
using ReMi.Commands.Auth;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.Auth;

namespace ReMi.CommandHandlers.Auth
{
    public class ProlongSessionHandler : IHandleCommand<ProlongSessionCommand>
    {
        public Func<IAccountsGateway> AccountsGatewayFactory { get; set; }
        public IApplicationSettings ApplicationSettings { get; set; }

        public void Handle(ProlongSessionCommand command)
        {
            using (var gateway = AccountsGatewayFactory())
            {
                gateway.ProlongSession(command.SessionId, ApplicationSettings.SessionDuration);
            }
        }
    }
}
