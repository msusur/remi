using System;

namespace ReMi.Common.WebApi.Notifications
{
    public interface INotificationFilterByReleaseTaskId : INotificationFilter
    {
        Guid ReleaseTaskId { get; }
    }
}
