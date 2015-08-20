using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessLogic.BusinessRules;
using ReMi.Commands.Auth;
using ReMi.Common.Constants.BusinessRules;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Plugins.Services.Authentication;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using System;
using ReMi.Common.Utils;
using BusinessAccount = ReMi.BusinessEntities.Auth.Account;

namespace ReMi.CommandHandlers.Auth
{
    public class StartSessionCommandHandler : IHandleCommand<StartSessionCommand>
    {
        public Func<IAccountsGateway> AccountsGatewayFactory { get; set; }
        public IAuthenticationService AuthenticationService { get; set; }
        public IBusinessRuleEngine BusinessRuleEngine { get; set; }

        public void Handle(StartSessionCommand command)
        {
            var serviceAccount = AuthenticationService.GetAccount(command.Login, command.Password);
            if (serviceAccount == null)
                throw new FailedToAuthenticateException(command.Login);

            using (var gateway = AccountsGatewayFactory())
            {
                var account = gateway.GetAccountByEmail(serviceAccount.Mail);
                if (account == null)
                {
                    var firstAccount = gateway.GetAccountsByRole("Admin").IsNullOrEmpty();
                    var newAccount = new BusinessAccount
                    {
                        Description = "Created automatically",
                        Email = serviceAccount.Mail,
                        FullName = serviceAccount.DisplayName,
                        Name = serviceAccount.Name,
                        Role = new Role { Name = firstAccount ? "Admin" : "BasicUser" },
                        ExternalId = serviceAccount.AccountId
                    };

                    account = gateway.CreateAccount(newAccount, false);
                }

                if (account == null || account.IsBlocked)
                    throw new FailedToAuthenticateException(command.Login);

                var session = gateway.StartSession(account, command.SessionId, SessionDuration());

                if (session == null)
                    throw new FailedToAuthenticateException(command.Login);
            }
        }

        private int SessionDuration()
        {
            return BusinessRuleEngine.Execute<int>(Guid.Empty, BusinessRuleConstants.Config.SessionDurationRule.ExternalId, null);
        }
    }
}
