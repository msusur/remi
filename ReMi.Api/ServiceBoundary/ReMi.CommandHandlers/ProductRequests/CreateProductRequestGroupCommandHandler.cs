using System;
using ReMi.BusinessEntities.Auth;
using ReMi.Commands.ProductRequests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.BusinessEntityGateways.ProductRequests;

namespace ReMi.CommandHandlers.ProductRequests
{
    public class CreateProductRequestGroupCommandHandler : IHandleCommand<CreateProductRequestGroupCommand>
    {
        public Func<IAccountsGateway> AccountsGatewayFactory { get; set; }
        public Func<IProductRequestGateway> ProductRequestGatewayFactory { get; set; }
        public Func<IProductRequestAssigneeGateway> ProductRequestAssigneeGatewayFactory { get; set; }

        public void Handle(CreateProductRequestGroupCommand command)
        {
            using (var gateway = ProductRequestGatewayFactory())
            {
                gateway.CreateProductRequestGroup(command.RequestGroup);
            }

            if (command.RequestGroup.Assignees != null)
                using (var gateway = ProductRequestAssigneeGatewayFactory())
                {
                    foreach (var assignee in command.RequestGroup.Assignees)
                    {
                        var acc = assignee;
                        var existingAccount = CreateNewAccountIfNotExist(acc);

                        gateway.AppendAssignee(command.RequestGroup.ExternalId, existingAccount.ExternalId);
                    }
                }
        }

        private Account CreateNewAccountIfNotExist(Account account)
        {
            using (var gateway = AccountsGatewayFactory())
            {
                var existingAccount = gateway.GetAccount(account.ExternalId);
                if (existingAccount == null)
                {

                    return gateway.CreateAccount(account, false);
                }
            }

            return account;
        }
    }
}
