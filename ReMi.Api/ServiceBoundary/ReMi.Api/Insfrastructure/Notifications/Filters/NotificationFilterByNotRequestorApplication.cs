using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.BusinessEntities.Auth;
using ReMi.Common.WebApi.Notifications;

namespace ReMi.Api.Insfrastructure.Notifications.Filters
{
    public class NotificationFilterByNotRequestorApplication : INotificationFilterByNotRequestorApplication
    {
        public bool Apply(INotificationFilterByNotRequestor filter, Account account, List<Subscription> subscriptions)
        {
            var result = null != subscriptions.SelectMany(o => o.Filters)
                .FirstOrDefault(o => account.ExternalId != filter.RequestorId);

            return result;
        }
    }
}
