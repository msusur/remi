using System;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.Subscriptions;
using ReMi.Queries.Subscriptions;

namespace ReMi.QueryHandlers.Subscriptions
{
    public class NotificationSubscriptionHandler 
        : IHandleQuery<GetAccountNotificationSubscriptionsRequest, GetAccountNotificationSubscriptionsResponse>
    {
        public Func<IAccountNotificationGateway> AccountNotificationGatewayFactory { get; set; }

        public GetAccountNotificationSubscriptionsResponse Handle(GetAccountNotificationSubscriptionsRequest request)
        {
            using (var gateway = AccountNotificationGatewayFactory())
            {
                return new GetAccountNotificationSubscriptionsResponse
                {
                    NotificationSubscriptions = gateway.GetAccountNotifications(request.AccountId)
                };
            }
        }
    }
}
