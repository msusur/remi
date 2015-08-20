using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.BusinessEntities.Auth;
using ReMi.Common.WebApi.Notifications;

namespace ReMi.Api.Insfrastructure.Notifications.Filters
{
    public class NotificationFilterForUserApplication : INotificationFilterForUserApplication
    {
        public bool Apply(INotificationFilterForUser filter, Account account, List<Subscription> subscriptions)
        {
            Guid v;

            var result = null != subscriptions.SelectMany(o => o.Filters)
                .Where(o => Guid.TryParse(o.Value, out v))
                .FirstOrDefault(o => filter.AccountId != Guid.Empty && account.ExternalId == filter.AccountId);

            return result;
        }
    }
}
