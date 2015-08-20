using System.Collections.Generic;
using ReMi.BusinessEntities.Auth;
using ReMi.Common.WebApi.Notifications;

namespace ReMi.Api.Insfrastructure.Notifications.Filters
{
    public class NotificationFilterApplication : INotificationFilter 
    {
        public bool Apply(INotificationFilter filter, Account account, List<Subscription> subscriptions)
        {
            return true;
        }
    }
}
