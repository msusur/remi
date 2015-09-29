using ReMi.Commands.Auth;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using System;

namespace ReMi.CommandHandlers.Auth
{
    public class UpdateAccountCommandHandler : IHandleCommand<UpdateAccountCommand>
    {
        public Func<IAccountsGateway> AccountsGatewayFactory { get; set; }

        public void Handle(UpdateAccountCommand request)
        {
            using (var gateway = AccountsGatewayFactory())
            {
                gateway.UpdateAccount(request.Account);
            }
        }
    }
}
