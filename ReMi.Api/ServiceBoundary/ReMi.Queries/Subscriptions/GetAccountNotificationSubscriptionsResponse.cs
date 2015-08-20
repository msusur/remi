using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.Subscriptions;
using ReMi.Common.Utils;

namespace ReMi.Queries.Subscriptions
{
    public class GetAccountNotificationSubscriptionsResponse
    {
        public IEnumerable<NotificationSubscription> NotificationSubscriptions { get; set; }

        public override string ToString()
        {
            return String.Format("[NotificationSubscriptions={0}]", NotificationSubscriptions.FormatElements());
        }
    }
}
