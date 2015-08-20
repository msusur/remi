using ReMi.Contracts.Plugins.Data;
using ReMi.Contracts.Plugins.Data.Email;
using ReMi.Contracts.Plugins.Services.Email;
using System.Collections.Generic;

namespace ReMi.Plugin.Composites.Services
{
    public class EmailServiceComposite : BaseComposit<IEmailService>, IEmailService
    {
        public void Send(IEnumerable<string> addressList, string subject, string body)
        {
            var service = GetPluginService(PluginType.Email);
            if (service != null) service.Send(addressList, subject, body);
        }

        public void Send(string address, string subject, string body)
        {
            var service = GetPluginService(PluginType.Email);
            if (service != null) service.Send(address, subject, body);
        }

        public void SendWithCalendarEvent(string address, string subject, string body, CalendarEvent calendarEvent, int releaseDuration)
        {
            var service = GetPluginService(PluginType.Email);
            if (service != null) service.SendWithCalendarEvent(address, subject, body, calendarEvent, releaseDuration);
        }
    }
}
