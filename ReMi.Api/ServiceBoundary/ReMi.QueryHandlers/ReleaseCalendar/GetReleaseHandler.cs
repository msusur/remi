using System;
using System.Linq;
using Common.Logging;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.Exceptions;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.Exceptions;
using ReMi.Queries.ReleaseCalendar;

namespace ReMi.QueryHandlers.ReleaseCalendar
{
    public class GetReleaseHandler : IHandleQuery<GetReleaseRequest, GetReleaseResponse>
    {
        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }
        public Func<IAccountsGateway> AccountGatewayFactory { get; set; }

        private static ILog Log = LogManager.GetCurrentClassLogger();

        public GetReleaseResponse Handle(GetReleaseRequest request)
        {
            if (request.Context == null || request.Context.UserId == Guid.Empty)
            {
                Log.WarnFormat("Requested release not allowed when current account is not determined. ReleaseWindowId={0}", request.ReleaseWindowId);
                throw new ReleaseWindowNotAllowedException(request.ReleaseWindowId, "account is not determined");
            }
            Account account;
            using (var gateway = AccountGatewayFactory())
            {
                account = gateway.GetAccount(request.Context.UserId);
            }
            using (var gateway = ReleaseWindowGatewayFactory())
            {
                var releaseWindow = gateway.GetByExternalId(request.ReleaseWindowId);
                if (releaseWindow == null)
                    throw new ReleaseNotFoundException(request.ReleaseWindowId);

                if (account.Products.All(x => releaseWindow.Products.All(o => o != x.Name)))
                {
                    Log.WarnFormat(
                        "Product of requested release not allowed for current account. ReleaseWindow={0}, Account={1}",
                        releaseWindow, account);

                    throw new ReleaseWindowNotAllowedException(request.ReleaseWindowId, "product not allowed");
                }
                return new GetReleaseResponse
                {
                    ReleaseWindow = releaseWindow
                };
            }
        }
    }
}
