using System;

namespace ReMi.Common.WebApi.Notifications
{
    public interface INotificationFilterForUser : INotificationFilter
    {
        Guid AccountId{ get; }
    }
}
