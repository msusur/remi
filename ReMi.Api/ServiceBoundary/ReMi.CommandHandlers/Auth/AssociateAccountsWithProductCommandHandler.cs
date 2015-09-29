using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessLogic.BusinessRules;
using ReMi.Commands.Auth;
using ReMi.Common.Constants.BusinessRules;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.Auth;

namespace ReMi.CommandHandlers.Auth
{
    public class AssociateAccountsWithProductCommandHandler : IHandleCommand<AssociateAccountsWithProductCommand>
    {
        public Func<IAccountsGateway> AccountsGatewayFactory { get; set; }
        public IBusinessRuleEngine BusinessRuleEngine { get; set; }

        public void Handle(AssociateAccountsWithProductCommand command)
        {
            using (var gateway = AccountsGatewayFactory())
            {
                gateway.AssociateAccountsWithProduct(command.Accounts.Select(x => x.Email), command.ReleaseWindowId,
                    GetRule(command.CommandContext.UserId));
            }
        }

        private Func<string, TeamRoleRuleResult> GetRule(Guid userId)
        {
            return s => BusinessRuleEngine.Execute<TeamRoleRuleResult>(
                userId, BusinessRuleConstants.Config.TeamRoleRule.ExternalId,
                new Dictionary<string, object> {{"roleName", s}});
        }
    }
}
