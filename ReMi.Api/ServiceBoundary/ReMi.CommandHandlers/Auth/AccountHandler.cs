using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.BusinessEntities.Auth;
using ReMi.Commands.Auth;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.Auth;

namespace ReMi.CommandHandlers.Auth
{
    public class AccountHandler : IHandleCommand<UpdateAccountCommand>,
        IHandleCommand<CreateAccountCommand>,
        IHandleCommand<AssociateAccountsWithProductCommand>,
        IHandleCommand<CheckAccountsCommand>
    {
        public Func<IAccountsGateway> AccountsGateway { get; set; }
        public ICommandDispatcher CommandDispatcher { get; set; }

        public void Handle(UpdateAccountCommand request)
        {
            using (var gateway = AccountsGateway())
            {
                gateway.UpdateAccount(request.Account);
            }
        }

        public void Handle(CreateAccountCommand request)
        {
            using (var gateway = AccountsGateway())
            {
                gateway.CreateAccount(request.Account);
            }
        }

        public void Handle(AssociateAccountsWithProductCommand command)
        {
            using (var gateway = AccountsGateway())
            {
                gateway.AssociateAccountsWithProduct(command.Accounts.Select(x => x.Email), command.ReleaseWindowId);
            }
        }

        public void Handle(CheckAccountsCommand command)
        {
            var newAccounts = new List<Account>();

            using (var gateway = AccountsGateway())
            {
                newAccounts.AddRange(
                    command.Accounts.Where(account => gateway.CreateNotExistingAccount(account))
                );
            }

            if (newAccounts.Count > 0)
                CommandDispatcher.Send(new AssociateAccountsWithProductCommand
                {
                    Accounts = newAccounts,
                    ReleaseWindowId = command.ReleaseWindowId
                });
        }
    }
}
