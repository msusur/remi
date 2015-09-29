using AutoMapper;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessLogic.BusinessRules;
using ReMi.Common.Constants.BusinessRules;
using ReMi.Contracts.Plugins.Services.Authentication;
using Account = ReMi.BusinessEntities.Auth.Account;
using Role = ReMi.BusinessEntities.Auth.Role;
using Session = ReMi.BusinessEntities.Auth.Session;

namespace ReMi.BusinessLogic.Auth
{
    public class AccountsBusinessLogic : IAccountsBusinessLogic
    {
        public Func<IAccountsGateway> AccountsGatewayFactory { get; set; }
        public Func<IProductGateway> ProductGatewayFactory { get; set; }

        public IAuthenticationService AuthenticationService { get; set; }
        public IMappingEngine Mapper { get; set; }
        public IBusinessRuleEngine BusinessRuleEngine { get; set; }

        public Session SignSession(string accountName, string password)
        {
            var account = AuthenticationService.GetAccount(accountName, password);
            if (account == null)
                return null;

            using (var gateway = AccountsGatewayFactory())
            {
                var existingAccount = gateway.GetAccountByEmail(account.Mail);
                if (existingAccount == null)
                    throw new AccountEmailNotFoundException(account.Mail);

                if (existingAccount.IsBlocked)
                    throw new AccountIsBlockedException(existingAccount.ExternalId);

                return gateway.SignSession(existingAccount);
            }
        }

        public List<Account> GetAccounts()
        {
            using (var gateway = AccountsGatewayFactory())
            {
                return gateway.GetAccounts().ToList();
            }
        }

        public List<Account> GetAccountsByRole(string roleName)
        {
            using (var gateway = AccountsGatewayFactory())
            {
                return gateway.GetAccountsByRole(roleName).ToList();
            }
        }

        public Session GetSession(Guid externalId)
        {
            using (var gateway = AccountsGatewayFactory())
            {
                return gateway.GetSession(externalId);
            }
        }

        public void UpdateAccount(Account account)
        {
            using (var gateway = AccountsGatewayFactory())
            {
                gateway.UpdateAccount(account);
            }
        }

        public List<Account> SearchAccounts(string criteria)
        {
            List<Account> existingAccounts;
            using (var gateway = AccountsGatewayFactory())
            {
                existingAccounts = gateway.GetAccounts().ToList();
            }

            return AuthenticationService
                .Search(criteria)
                .Where(x => !existingAccounts.Any(exacc => String.Equals(exacc.Email, x.Mail, StringComparison.CurrentCultureIgnoreCase)))
                .Select(x => new Account
                {
                    Description = "Created automatically",
                    Email = x.Mail,
                    Name = x.Name,
                    FullName = x.DisplayName,
                    ExternalId = x.AccountId,
                    Role = new Role { Name = "BasicUser" }
                })
                .ToList();
        }

        public void AssociateAccountsWithProduct(IEnumerable<string> accountEmails, Guid releaseWindowId, Guid userId)
        {
            using (var productGateway = ProductGatewayFactory())
            {
                var productIds = productGateway.GetProducts(releaseWindowId).Select(o => o.ExternalId).ToList();

                using (var accountGateway = AccountsGatewayFactory())
                {
                    accountGateway.AssociateAccountsWithProducts(productIds, accountEmails, GetRule(userId));
                }
            }
        }

        public void CreateAccount(Account account)
        {
            using (var gateway = AccountsGatewayFactory())
            {
                gateway.CreateAccount(account);
            }
        }

        private Func<string, TeamRoleRuleResult> GetRule(Guid userId)
        {
            return s => BusinessRuleEngine.Execute<TeamRoleRuleResult>(
                userId, BusinessRuleConstants.Config.TeamRoleRule.ExternalId,
                new Dictionary<string, object> { { "roleName", s } });
        }
    }
}
