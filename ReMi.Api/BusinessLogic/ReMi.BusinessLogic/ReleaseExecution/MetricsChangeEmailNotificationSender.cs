using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Common.Constants.ReleaseExecution;
using ReMi.Common.Constants.Subscriptions;
using ReMi.Common.Utils;
using ReMi.Contracts.Plugins.Services.Email;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.Subscriptions;
using ReMi.Events.Metrics;

namespace ReMi.BusinessLogic.ReleaseExecution
{
    public class MetricsChangeEmailNotificationSender : IMetricsChangeEmailNotificationSender
    {
        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }
        public Func<IAccountNotificationGateway> AccountNotificationGatewayFactory { get; set; }
        public IEmailTextProvider EmailTextProvider { get; set; }
        public IEmailService EmailService { get; set; }
        public IApplicationSettings ApplicationSettings { get; set; }

        public void Send(MetricsUpdatedEvent evnt, MetricType metric, string emailSubjectTemplate)
        {
            if (evnt.Metric.MetricType != metric) return;

            ReleaseWindow releaseWindow;
            using (var gateway = ReleaseWindowGatewayFactory())
            {
                releaseWindow = gateway.GetByExternalId(evnt.ReleaseWindowId, true);
            }
            IEnumerable<Account> subscribers;
            using (var gateway = AccountNotificationGatewayFactory())
            {
                subscribers = gateway.GetSubscribers(
                    (NotificationType)Enum.Parse(typeof(NotificationType), metric.ToString()),
                    releaseWindow.Products);
            }
            if (subscribers.IsNullOrEmpty()) return;

            var products = releaseWindow.Products.FormatElements(string.Empty, string.Empty);
            var emailSubject = string.Format(emailSubjectTemplate, products);

            foreach (var subscriber in subscribers)
            {
                var replaceValues = new Dictionary<string, object>
                {
                    {"Recipient", subscriber.FullName},
                    {"Product", products},
                    {"Sprint", releaseWindow.Sprint},
                    {
                        "ReleaseUrl", string.Format("{0}release?{1}={2}&tab=execution",
                            ApplicationSettings.FrontEndUrl, "releaseWindowId", releaseWindow.ExternalId)
                    }
                };

                var email = EmailTextProvider.GetText(metric.ToString(), replaceValues);

                EmailService.Send(subscriber.Email, emailSubject, email);
            }
        }
    }
}
