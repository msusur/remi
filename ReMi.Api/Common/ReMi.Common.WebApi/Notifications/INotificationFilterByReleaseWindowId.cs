using System;

namespace ReMi.Common.WebApi.Notifications
{
    public interface INotificationFilterByReleaseWindowId : INotificationFilter
    {
        Guid ReleaseWindowId { get; }
    }
}
