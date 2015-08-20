using System;
using ReMi.Commands.Auth;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.Auth;

namespace ReMi.CommandHandlers.Auth
{
    public class UpdateAccountPackagesCommandHandler : IHandleCommand<UpdateAccountPackagesCommand>
    {
        public Func<IAccountsGateway> AccountsGatewayFactory { get; set; }

        public void Handle(UpdateAccountPackagesCommand command)
        {
            using (var gateway = AccountsGatewayFactory())
            {
                gateway.UpdateAccountPackages(command.AccountId, command.PackageIds, command.DefaultPackageId);
            }
        }
    }
}
