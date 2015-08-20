using System.Text;
using ReMi.BusinessEntities.Auth;
using ReMi.Commands.Auth;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.Queries.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web.Http.Controllers;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Contracts.Plugins.Services.Authentication;

namespace ReMi.Api.Insfrastructure.Security
{
    public class AuthorizationManager : IAuthorizationManager
    {
        private IPrincipal _principal;
        private Account _account;

        public IAuthenticationService AuthenticationService { get; set; }
        public IHandleQuery<GetActiveSessionRequest, GetActiveSessionResponse> ActiveSessionQuery { get; set; }
        public IPrincipalSetter PrincipalSetter { get; set; }
        public Func<IAccountsGateway> AccountsGatewayFactory { get; set; }
        public ICommandDispatcher CommandDispatcher { get; set; }

        public Account Authenticate(HttpActionContext actionContext)
        {
            _account = GetAccount(actionContext);
            if (_account != null)
                _principal = new RequestPrincipal(_account);
            else
                _principal = new GenericPrincipal(new GenericIdentity(string.Empty), new string[0]);

            PrincipalSetter.SetPrincipal(_principal, actionContext);
            return _account;
        }

        public bool IsAuthorized(IEnumerable<Role> roles)
        {
            if (roles != null && roles.Any(o => o.Name == "NotAuthenticated"))
                return true;

            return _principal != null && _account != null && _principal.IsInRole(roles, _account);
        }

        private Account GetAccount(HttpActionContext actionContext)
        {
            var token = GetAuthorizationToken(actionContext);
            if (string.IsNullOrWhiteSpace(token))
            {
                var basic = ParseAuthorizationHeader(actionContext);
                if (basic == null || string.IsNullOrEmpty(basic.Username)) return null;

                var account = AuthenticationService.GetAccount(basic.Username, basic.Password);
                if (account == null) return null;

                using (var gateway = AccountsGatewayFactory())
                {
                    return gateway.GetAccountByEmail(account.Mail);
                }
            }
            
            var parts = HttpTokenHelper.GetTokenParts(token);
            if (parts == null)
                return null;
            var request = new GetActiveSessionRequest { SessionId = parts.SessionId, UserName = parts.UserName };
            var response = ActiveSessionQuery.Handle(request);
            if (response != null)
            {
                CommandDispatcher.Send(new ProlongSessionCommand
                {
                    SessionId = request.SessionId,
                    CommandContext = new CommandContext
                    {
                        Id = Guid.NewGuid()
                    }
                });
            }

            return response != null ? response.Account : null;
        }

        private static string GetAuthorizationToken(HttpActionContext actionContext)
        {
            if (actionContext.Request.Headers.Authorization != null
                &&
                Consts.HeaderSchema.Equals(actionContext.Request.Headers.Authorization.Scheme,
                    StringComparison.InvariantCultureIgnoreCase))

                return actionContext.Request.Headers.Authorization.Parameter;

            return null;
        }

        private static BasicAuthCredentials ParseAuthorizationHeader(HttpActionContext actionContext)
        {
            string authHeader = null;
            var auth = actionContext.Request.Headers.Authorization;
            if (auth != null && auth.Scheme == "Basic")
                authHeader = auth.Parameter;

            if (string.IsNullOrEmpty(authHeader))
                return null;

            authHeader = Encoding.Default.GetString(Convert.FromBase64String(authHeader));

            var tokens = authHeader.Split(':');
            if (tokens.Length < 2)
                return null;

            return new BasicAuthCredentials()
            {
                Username = tokens[0],
                Password = tokens[1]
            };
        }

        private class BasicAuthCredentials
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}
