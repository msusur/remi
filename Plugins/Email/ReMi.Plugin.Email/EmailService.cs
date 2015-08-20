using AutoMapper;
using ReMi.Contracts.Plugins.Data.Email;
using ReMi.Contracts.Plugins.Services.Email;
using ReMi.Plugin.Email.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace ReMi.Plugin.Email
{
    public class EmailService : IEmailService
    { 
        private readonly PluginConfigurationEntity _config = new PluginConfigurationEntity();
        public IMappingEngine Mapper { get; set; }

        public virtual void Send(IEnumerable<string> addressList, string subject, string body)
        {
            //var config = Configuration.GetPluginConfiguration();

            var mail = ConstructMailMessage(addressList, subject, _config.UserName);
            mail.Body = body;

            var client = ConstructSmtpClient(_config.Smtp, _config.UserName, _config.Password);

            DisableServerSertificateValidation();

            client.Send(mail);
        }

        public void Send(string address, string subject, string body)
        {
            Send(new List<string> {address}, subject, body);
        }

        public virtual void SendWithCalendarEvent(string address, string subject, string body, CalendarEvent calendarEvent, int releaseDuration)
        {
            //var config = Configuration.GetPluginConfiguration();

            var mail = ConstructMailMessage(new List<string> { address }, subject, _config.UserName);

            var client = ConstructSmtpClient(_config.Smtp, _config.UserName, _config.Password);

            var outlookEvent = Mapper.Map<CalendarEvent, OutlookEventEntity>(calendarEvent);

            new IcsConstructor().Build(outlookEvent, mail, body, releaseDuration,
                string.IsNullOrWhiteSpace(outlookEvent.Organizer) ? _config.UserName : outlookEvent.Organizer);

            DisableServerSertificateValidation();

            client.Send(mail);
        }

        private MailMessage ConstructMailMessage(IEnumerable<string> addressList, string subject, string user)
        {
            var mail = new MailMessage
            {
                From = new MailAddress(user),
                Subject = subject,
                IsBodyHtml = true
            };

            addressList.ToList().ForEach(address => mail.To.Add(address));

            return mail;
        }

        private SmtpClient ConstructSmtpClient(string smtp, string user, string password)
        {
            return new SmtpClient
            {
                Host = smtp,
                UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = true,
                Credentials = new NetworkCredential(user, password)
            };
        }

        private void DisableServerSertificateValidation()
        {
            ServicePointManager.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;
        }
    }
}
