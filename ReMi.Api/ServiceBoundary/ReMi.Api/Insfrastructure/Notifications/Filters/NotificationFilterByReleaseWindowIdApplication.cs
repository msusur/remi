using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.BusinessEntities.Auth;
using ReMi.Common.Utils;
using ReMi.Common.WebApi.Notifications;

namespace ReMi.Api.Insfrastructure.Notifications.Filters
{
    public class NotificationFilterByReleaseWindowIdApplication : INotificationFilterByReleaseWindowIdApplication
    {
        public bool Apply(INotificationFilterByReleaseWindowId filter, Account account, List<Subscription> subscriptions)
        {
            Guid v;

            var result = null != subscriptions.SelectMany(o => o.Filters)
                .Where(o => o.Property.Equals(filter.PropertyName(p => p.ReleaseWindowId), StringComparison.InvariantCultureIgnoreCase))
                .Where(o => Guid.TryParse(o.Value, out v))
                .FirstOrDefault(o => Guid.Parse(o.Value) == filter.ReleaseWindowId);

            return result;
        }
    }
}
