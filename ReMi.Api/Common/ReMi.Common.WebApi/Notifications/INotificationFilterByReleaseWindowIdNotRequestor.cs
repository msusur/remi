using System;
using ReMi.BusinessEntities.Auth;

namespace ReMi.Common.WebApi.Notifications
{
    public interface INotificationFilterByReleaseWindowIdNotRequestor : INotificationFilter
    {
        Guid ReleaseWindowId { get; }
        Guid RequestorId { get; }
    }
}
