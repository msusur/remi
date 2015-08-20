using System;
using AutoMapper;
using Common.Logging;
using ReMi.Common.Cqrs;
using ReMi.BusinessLogic.Auth;
using ReMi.Queries.Auth;
using BusinessAccount = ReMi.BusinessEntities.Auth.Account;
using ApiAccount = ReMi.Api.Models.Auth.Account;
using BusinessSession = ReMi.BusinessEntities.Auth.Session;
using ApiSession = ReMi.Api.Models.Auth.Session;

namespace ReMi.QueryHandlers.Auth
{
    public class GetActiveSessionHandler : IHandleQuery<GetActiveSessionRequest, GetActiveSessionResponse>
    {
        private static readonly ILog _logger = LogManager.GetCurrentClassLogger();


        private readonly Func<IAccountsBusinessLogic> _accountsBusinessLogicFactory;
        private readonly IMappingEngine _mappingEngine;

        #region .ctor

        public GetActiveSessionHandler(Func<IAccountsBusinessLogic> accountsBusinessLogicFactory,
            IMappingEngine mappingEngine)
        {
            _accountsBusinessLogicFactory = accountsBusinessLogicFactory;
            _mappingEngine = mappingEngine;
        }

        #endregion

        public GetActiveSessionResponse Handle(GetActiveSessionRequest request)
        {
            IAccountsBusinessLogic accountBusinessLogic = _accountsBusinessLogicFactory();

            BusinessSession session = accountBusinessLogic.GetSession(request.SessionId);

            if (session == null || session.Completed.HasValue ||
                (session.ExpireAfter.HasValue && session.ExpireAfter <= DateTime.UtcNow))
                return null;

            BusinessAccount account = accountBusinessLogic.GetAccount(session.AccountId);

            if (account == null || account.IsBlocked)
                return null;

            ApiAccount apiAccount = _mappingEngine.Map<BusinessAccount, ApiAccount>(account);
            ApiSession apiSession = _mappingEngine.Map<BusinessSession, ApiSession>(session);

            return new GetActiveSessionResponse {Account = apiAccount, Session = apiSession};
        }
    }
}
