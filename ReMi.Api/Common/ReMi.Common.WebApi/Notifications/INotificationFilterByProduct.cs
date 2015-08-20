using System.Collections.Generic;

namespace ReMi.Common.WebApi.Notifications
{
    public interface INotificationFilterByProduct : INotificationFilter
    {
        IEnumerable<string> Products { get; }
    }
}
