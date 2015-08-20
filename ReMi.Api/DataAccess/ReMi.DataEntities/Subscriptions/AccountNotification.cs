using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Common.Constants.Subscriptions;
using ReMi.DataEntities.Auth;

namespace ReMi.DataEntities.Subscriptions
{
    [Table("AccountNotification", Schema = Constants.SchemaName)]
    public class AccountNotification
    {
        [Key]
        public int AccountNotificationId { get; set; }

        [Index("IX_AccountNotification", 1)]
        public int AccountId { get; set; }

        [Index("IX_AccountNotification", 2)]
        public NotificationType NotificationType { get; set; }

        public virtual Account Account { get; set; }

        public override string ToString()
        {
            return String.Format("[AccountNotificationId={0}, AccountId={1}, NotificationType={2}]",
                AccountNotificationId, AccountId, NotificationType);
        }
    }
}
