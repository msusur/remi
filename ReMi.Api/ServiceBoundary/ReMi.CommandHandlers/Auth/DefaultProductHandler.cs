using System;
using System.Linq;
using ReMi.BusinessLogic.Exceptions;
using ReMi.Commands.Auth;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.Auth;

namespace ReMi.CommandHandlers.Auth
{
    public class DefaultProductHandler : IHandleCommand<SetDefaultProductForNewlyRegisteredUserCommand>
    {
        public Func<IAccountsGateway> AccountsGatewayFactory { get; set; }

        public void Handle(SetDefaultProductForNewlyRegisteredUserCommand command)
        {
            using (var accGateway = AccountsGatewayFactory())
            {
                var sessions = accGateway.GetSessions(command.Account.ExternalId);

                if (sessions.Count() != 1)
                {
                    throw new UserAlreadyRegistedException(command.Account.ExternalId);
                }

                accGateway.UpdateAccountProducts(command.Account);
            }
        }
    }
}
