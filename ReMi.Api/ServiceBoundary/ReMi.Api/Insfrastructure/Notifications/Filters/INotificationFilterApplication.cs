using System.Collections.Generic;
using ReMi.BusinessEntities.Auth;
using ReMi.Common.WebApi.Notifications;

namespace ReMi.Api.Insfrastructure.Notifications.Filters
{
    public interface INotificationFilterApplication<T> where T : INotificationFilter
    {
        bool Apply(T filter, Account account, List<Subscription> subscriptions);
    }
}
