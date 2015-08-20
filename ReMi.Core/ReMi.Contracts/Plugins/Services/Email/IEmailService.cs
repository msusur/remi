using System.Collections.Generic;
using ReMi.Contracts.Plugins.Data.Email;

namespace ReMi.Contracts.Plugins.Services.Email
{
    public interface IEmailService : IPluginService
    {
        void Send(IEnumerable<string> addressList, string subject, string body);
        void Send(string address, string subject, string body);
        void SendWithCalendarEvent(string address, string subject, string body, CalendarEvent calendarEvent, int releaseDuration);
    }
}
