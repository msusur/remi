using System;
using System.Collections.Generic;
using ReMi.Common.Constants.Subscriptions;
using ReMi.Common.Utils;

namespace ReMi.DataAccess.Exceptions.Subscriptions
{
    public class DuplicatedNotificationSubscriptionException : ApplicationException
    {
        public DuplicatedNotificationSubscriptionException(IEnumerable<NotificationType> notificationTypes, Guid accountId)
            : base(FormatMessage(notificationTypes, accountId))
		{
		}

        public DuplicatedNotificationSubscriptionException(IEnumerable<NotificationType> notificationTypes, Guid accountId, Exception innerException)
            : base(FormatMessage(notificationTypes, accountId), innerException)
		{
		}

        private static string FormatMessage(IEnumerable<NotificationType> notificationTypes, Guid accountId)
        {
            return string.Format("Subscription on notifications: {0} for account: {1} already exists", notificationTypes.FormatElements(),
                accountId);
        }
    }
}
