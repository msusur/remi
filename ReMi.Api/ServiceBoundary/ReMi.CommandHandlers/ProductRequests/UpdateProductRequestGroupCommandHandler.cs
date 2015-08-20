using System;
using System.Linq;
using ReMi.BusinessEntities.Auth;
using ReMi.Commands.ProductRequests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.BusinessEntityGateways.ProductRequests;

namespace ReMi.CommandHandlers.ProductRequests
{
    public class UpdateProductRequestGroupCommandHandler : IHandleCommand<UpdateProductRequestGroupCommand>
    {
        public Func<IAccountsGateway> AccountsGatewayFactory { get; set; }
        public Func<IProductRequestGateway> ProductRequestGatewayFactory { get; set; }
        public Func<IProductRequestAssigneeGateway> ProductRequestAssigneeGatewayFactory { get; set; }

        public void Handle(UpdateProductRequestGroupCommand command)
        {
            using (var gateway = ProductRequestGatewayFactory())
            {
                gateway.UpdateProductRequestGroup(command.RequestGroup);
            }

            using (var gateway = ProductRequestAssigneeGatewayFactory())
            {
                var assignees = gateway.GetAssignees(command.RequestGroup.ExternalId).ToList();
                var oldAssignees = assignees;
                if (command.RequestGroup.Assignees != null)
                    oldAssignees = oldAssignees.Where(
                        o => !command.RequestGroup.Assignees.Select(x => x.ExternalId).Contains(o.ExternalId)).ToList();

                foreach (var oldAssignee in oldAssignees)
                {
                    gateway.RemoveAssignee(command.RequestGroup.ExternalId, oldAssignee.ExternalId);
                }

                if (command.RequestGroup.Assignees != null)
                {
                    var newAssignees =
                        command.RequestGroup.Assignees.Where(
                            o =>
                                !assignees.Select(x => x.ExternalId).Contains(o.ExternalId));

                    foreach (var newAssignee in newAssignees)
                    {
                        var checkedAccount = CreateNewAccountIfNotExist(newAssignee);

                        gateway.AppendAssignee(command.RequestGroup.ExternalId, checkedAccount.ExternalId);
                    }
                }
            }
        }

        private Account CreateNewAccountIfNotExist(Account account)
        {
            using (var gateway = AccountsGatewayFactory())
            {
                var existingAccount = gateway.GetAccount(account.ExternalId);
                if (existingAccount == null)
                    return gateway.CreateAccount(account, false);
            }

            return account;
        }
    }
}
