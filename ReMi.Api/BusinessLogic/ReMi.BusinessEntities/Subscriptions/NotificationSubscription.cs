using System;
using ReMi.Common.Constants.Subscriptions;

namespace ReMi.BusinessEntities.Subscriptions
{
    public class NotificationSubscription
    {
        public String NotificationName { get; set; }
        public bool Subscribed { get; set; }
        public NotificationType NotificationType { get; set; }

        public override string ToString()
        {
            return String.Format("[NotificationName={0},Subscribed={1}, NotificationType={2}]",
                NotificationName, Subscribed, NotificationType);
        }
    }
}
