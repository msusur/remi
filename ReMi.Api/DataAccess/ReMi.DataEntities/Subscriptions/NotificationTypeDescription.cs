using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Common.Constants;
using ReMi.Common.Utils.Enums;

namespace ReMi.DataEntities.Subscriptions
{
    [Table("NotificationType", Schema = Constants.SchemaName)]
    public class NotificationTypeDescription : EnumDescription
    {
    }
}
