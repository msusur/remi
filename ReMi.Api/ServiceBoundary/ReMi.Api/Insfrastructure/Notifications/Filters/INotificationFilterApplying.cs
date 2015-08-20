using System.Collections.Generic;
using ReMi.BusinessEntities.Auth;
using ReMi.Common.Cqrs;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Api.Insfrastructure.Notifications.Filters
{
    public interface INotificationFilterApplying
    {
        bool Apply(IEvent evnt, Account account, List<Subscription> subscriptions);
    }
}
