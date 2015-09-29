using ReMi.Commands.Auth;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using System;

namespace ReMi.CommandHandlers.Auth
{
    public class CreateAccountCommandHandler : IHandleCommand<CreateAccountCommand>
    {
        public Func<IAccountsGateway> AccountsGatewayFactory { get; set; }

        public void Handle(CreateAccountCommand command)
        {
            using (var gateway = AccountsGatewayFactory())
            {
                gateway.CreateAccount(command.Account);
            }
        }
    }
}
