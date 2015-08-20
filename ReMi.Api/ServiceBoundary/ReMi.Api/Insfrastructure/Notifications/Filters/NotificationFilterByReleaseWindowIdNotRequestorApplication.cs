using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.BusinessEntities.Auth;
using ReMi.Common.Utils;
using ReMi.Common.WebApi.Notifications;

namespace ReMi.Api.Insfrastructure.Notifications.Filters
{
    public class NotificationFilterByReleaseWindowIdNotRequestorApplication : INotificationFilterByReleaseWindowIdNotRequestorApplication
    {
        public bool Apply(INotificationFilterByReleaseWindowIdNotRequestor filter, Account account, List<Subscription> subscriptions)
        {
            Guid v;

            var result = null != subscriptions.SelectMany(o => o.Filters)
                .Where(o => o.Property.Equals(filter.PropertyName(p => p.ReleaseWindowId), StringComparison.InvariantCultureIgnoreCase))
                .Where(o => Guid.TryParse(o.Value, out v))
                .Where(o => account.ExternalId != filter.RequestorId)
                .FirstOrDefault(o => Guid.Parse(o.Value) == filter.ReleaseWindowId);

            return result;
        }
    }
}
