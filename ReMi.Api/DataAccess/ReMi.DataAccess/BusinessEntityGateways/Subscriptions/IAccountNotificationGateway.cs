using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.Subscriptions;
using ReMi.Common.Constants.Subscriptions;

namespace ReMi.DataAccess.BusinessEntityGateways.Subscriptions
{
    public interface IAccountNotificationGateway : IDisposable
    {
        IEnumerable<NotificationSubscription> GetAccountNotifications(Guid accountId);
        IEnumerable<Account> GetSubscribers(NotificationType notificationType);
        IEnumerable<Account> GetSubscribers(NotificationType notificationType, IEnumerable<string> products);

        void RemoveNotificationSubscriptions(Guid accountId, IEnumerable<NotificationType> notificationTypes);
        void AddNotificationSubscriptions(Guid accountId, IEnumerable<NotificationType> notificationTypes);
    }
}
