using ReMi.BusinessEntities.Auth;
using ReMi.Commands.Auth;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.Common.Utils;

namespace ReMi.CommandHandlers.Auth
{
    public class CheckAccountsCommandHandler : IHandleCommand<CheckAccountsCommand>
    {
        public Func<IAccountsGateway> AccountsGatewayFactory { get; set; }
        public ICommandDispatcher CommandDispatcher { get; set; }

        public void Handle(CheckAccountsCommand command)
        {
            var newAccounts = new List<Account>();

            using (var gateway = AccountsGatewayFactory())
            {
                newAccounts.AddRange(
                    command.Accounts.Where(account => gateway.CreateNotExistingAccount(account))
                    );
            }

            if (newAccounts.Count > 0)
                CommandDispatcher.Send(new AssociateAccountsWithProductCommand
                {
                    Accounts = newAccounts,
                    ReleaseWindowId = command.ReleaseWindowId,
                    CommandContext = command.CommandContext.CreateChild()
                });
        }
    }
}
