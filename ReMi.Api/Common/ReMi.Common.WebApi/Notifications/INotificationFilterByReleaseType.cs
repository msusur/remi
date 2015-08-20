using ReMi.Common.Constants.ReleaseCalendar;

namespace ReMi.Common.WebApi.Notifications
{
    public interface INotificationFilterByReleaseType : INotificationFilter
    {
        ReleaseType ReleaseType { get; }
    }
}
