using System;
using System.Web.Http;
using ReMi.Common.WebApi;
using ReMi.Queries.Subscriptions;

namespace ReMi.Api.Controllers
{
    [RoutePrefix("subscriptions")]
    public class SubscriptionController : ApiController
    {
        public
            IImplementQueryAction
                <GetAccountNotificationSubscriptionsRequest, GetAccountNotificationSubscriptionsResponse>
            NotificationSubscriptionAction { get; set; }

        [HttpGet]
        [Route("{accountId:guid}")]
        public GetAccountNotificationSubscriptionsResponse GetNotificationSubscriptions(Guid accountId)
        {
            var request = new GetAccountNotificationSubscriptionsRequest
            {
                AccountId = accountId
            };

            return NotificationSubscriptionAction.Handle(ActionContext, request);
        }
    }
}
