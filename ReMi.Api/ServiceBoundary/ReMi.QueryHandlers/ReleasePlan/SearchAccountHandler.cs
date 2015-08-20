using System;
using Common.Logging;
using ReMi.BusinessLogic.Auth;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Queries.Auth;
using ReMi.Queries.ReleasePlan;

namespace ReMi.QueryHandlers.ReleasePlan
{
    public class SearchAccountHandler : IHandleQuery<SearchAccountRequest, GetAccountsResponse>
    {
        public Func<IAccountsBusinessLogic> AccountsBusinessLogicFactory { get; set; }
        private readonly ILog _log = LogManager.GetCurrentClassLogger();

        public GetAccountsResponse Handle(SearchAccountRequest request)
        {
            var accountBusinessLogic = AccountsBusinessLogicFactory();

            var accounts = accountBusinessLogic.SearchAccounts(request.Criteria);

            return new GetAccountsResponse {Accounts = accounts};
        }
    }
}
