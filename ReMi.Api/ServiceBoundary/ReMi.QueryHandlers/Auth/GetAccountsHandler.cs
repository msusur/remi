using System;
using System.Linq;
using ReMi.BusinessLogic.Auth;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.Queries.Auth;
using BusinessAccount = ReMi.BusinessEntities.Auth.Account;
using BusinessSession = ReMi.BusinessEntities.Auth.Session;

namespace ReMi.QueryHandlers.Auth
{
    public class GetAccountsHandler :
        IHandleQuery<GetActiveSessionRequest, GetActiveSessionResponse>,
        IHandleQuery<GetAccountsRequest, GetAccountsResponse>,
        IHandleQuery<GetAccountsByProductRequest, GetAccountsByProductResponse>,
        IHandleQuery<GetAccountsByRoleRequest, GetAccountsByRoleResponse>
    {
        public Func<IAccountsGateway> AccountsGateway { get; set; }
        public IHandleQuery<GetAccountRequest, GetAccountResponse> GetAccountQuery { get; set; }
        
        public IAccountsBusinessLogic AccountsBusinessLogic { get; set; }
        
        public GetActiveSessionResponse Handle(GetActiveSessionRequest request)
        {
            var session = AccountsBusinessLogic.GetSession(request.SessionId);

            if (session == null || session.Completed.HasValue ||
                (session.ExpireAfter.HasValue && session.ExpireAfter <= SystemTime.Now))
                return null;

            var account =
                GetAccountQuery.Handle(new GetAccountRequest {AccountId = session.AccountId}).Account;

            if (account == null || account.IsBlocked)
                return null;

            return new GetActiveSessionResponse { Account = account, Session = session };
        }

        public GetAccountsResponse Handle(GetAccountsRequest request)
        {
            var accounts = AccountsBusinessLogic.GetAccounts();

            return new GetAccountsResponse { Accounts = accounts };
        }

        public GetAccountsByRoleResponse Handle(GetAccountsByRoleRequest request)
        {
            var accounts = AccountsBusinessLogic.GetAccountsByRole(request.Role);

            return new GetAccountsByRoleResponse { Accounts = accounts };
        }

        public GetAccountsByProductResponse Handle(GetAccountsByProductRequest request)
        {
            using (var gateway = AccountsGateway())
            {
                return new GetAccountsByProductResponse
                {
                    Accounts = gateway.GetAccounts()
                    .Where(o => o.Products.Any(x => x.Name.Equals(request.Product, StringComparison.InvariantCultureIgnoreCase)))
                };
            }
        }
    }
}
