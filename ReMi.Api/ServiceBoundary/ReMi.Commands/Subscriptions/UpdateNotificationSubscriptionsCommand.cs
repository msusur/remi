using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.Subscriptions;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.Subscriptions
{
    [Command("Updates notification subscriptions for specified account", CommandGroup.Subscriptions)]
    public class UpdateNotificationSubscriptionsCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }
        public List<NotificationSubscription> NotificationSubscriptions { get; set; }

        public override string ToString()
        {
            return String.Format("[NotificationSubscriptions={0}, CommandContext={1}]",
                NotificationSubscriptions.FormatElements(), CommandContext);
        }
    }
}
