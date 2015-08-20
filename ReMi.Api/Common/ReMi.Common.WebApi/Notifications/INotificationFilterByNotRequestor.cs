using System;

namespace ReMi.Common.WebApi.Notifications
{
    public interface INotificationFilterByNotRequestor : INotificationFilter
    {
        Guid RequestorId { get; }
    }
}
