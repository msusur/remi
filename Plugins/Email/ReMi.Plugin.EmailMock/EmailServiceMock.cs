using ReMi.Contracts.Plugins.Data.Email;
using ReMi.Plugin.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.Contracts.Plugins.Services;

namespace ReMi.Plugin.EmailMock
{
    public class EmailServiceMock : EmailService
    {
        public IPluginConfiguration<PluginConfigurationEntity> PluginConfiguration { get; set; }
        
        //public PluginConfiguration Configuration { get; set; }

        public override void Send(IEnumerable<string> addressList, string subject, string body)
        {
            var config = PluginConfiguration.GetPluginConfiguration();

            if (!string.IsNullOrEmpty(config.RedirectToEmail) && addressList.Any(o => !o.Equals(
               config.RedirectToEmail, StringComparison.InvariantCultureIgnoreCase)))
            {
                Send(
                    config.RedirectToEmail,
                    subject,
                    "Original addresses: " + string.Join(", ", addressList) + "<br/ >---------------------<br/ >" + body);

                return;
            }

            base.Send(addressList, subject, body);
        }

        public override void SendWithCalendarEvent(string address, string subject, string body, CalendarEvent outlookEvent, int releaseDuration)
        {
            var config = PluginConfiguration.GetPluginConfiguration();

            if (!string.IsNullOrEmpty(config.RedirectToEmail) && !address.Equals(config.RedirectToEmail, StringComparison.InvariantCultureIgnoreCase))
            {
                base.SendWithCalendarEvent(
                    config.RedirectToEmail,
                    subject,
                    "Original addresses: " + address + "<br/ >---------------------<br/ >" + body,
                    outlookEvent, 
                    releaseDuration);

                return;
            }

            base.SendWithCalendarEvent(address, subject, body, outlookEvent, releaseDuration);
        }
    }
}
