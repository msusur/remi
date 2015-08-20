using System;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.Queries.Auth;

namespace ReMi.QueryHandlers.Auth
{
    public class GetNewSessionHandler : IHandleQuery<GetNewSessionRequest, GetActiveSessionResponse>
    {
        public Func<IAccountsGateway> AccountsGatewayFactory { get; set; }

        public GetActiveSessionResponse Handle(GetNewSessionRequest request)
        {
            using (var gateway = AccountsGatewayFactory())
            {
                var session = gateway.GetSession(request.SessionId);

                if (session == null)
                    return null;

                var account = gateway.GetAccount(session.AccountId, true);

                if (account == null || account.IsBlocked)
                    return null;

                return new GetActiveSessionResponse
                {
                    Account = account,
                    Session = session
                };
            }
        }
    }
}
