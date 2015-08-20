using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.BusinessEntities.Subscriptions;
using ReMi.Commands.Subscriptions;
using ReMi.Common.Constants.Subscriptions;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.Subscriptions;

namespace ReMi.CommandHandlers.Subscriptions
{
    public class UpdateNotificationSubscriptionsHandler 
        : IHandleCommand<UpdateNotificationSubscriptionsCommand>
    {
        public Func<IAccountNotificationGateway> AccountNotificationGatwayFactory { get; set; }

        public void Handle(UpdateNotificationSubscriptionsCommand command)
        {
            using (var gateway = AccountNotificationGatwayFactory())
            {
                var notificationSubscriptions =
                    gateway.GetAccountNotifications(command.CommandContext.UserId);

                var notificationsToRemove = GetNoticationTypes(notificationSubscriptions, x =>
                    x.Subscribed &&
                    command.NotificationSubscriptions.Any(
                        c => c.NotificationType == x.NotificationType && !c.Subscribed));
                    
                if (notificationsToRemove.Any())
                {
                    gateway.RemoveNotificationSubscriptions(command.CommandContext.UserId,
                        notificationsToRemove);
                }

                var notificationsToAdd = GetNoticationTypes(notificationSubscriptions,  x =>
                    !x.Subscribed &&
                    command.NotificationSubscriptions.Any(
                        c => c.NotificationType == x.NotificationType && c.Subscribed));

                if (notificationsToAdd.Any())
                {
                    gateway.AddNotificationSubscriptions(command.CommandContext.UserId,
                        notificationsToAdd);
                }
            }
        }

        private List<NotificationType> GetNoticationTypes(IEnumerable<NotificationSubscription> notificationSubscriptions,
            Func<NotificationSubscription, bool> criteria)
        {
            return notificationSubscriptions.Where(criteria)
                .Select(
                    s => s.NotificationType).ToList();
        }
    }
}
