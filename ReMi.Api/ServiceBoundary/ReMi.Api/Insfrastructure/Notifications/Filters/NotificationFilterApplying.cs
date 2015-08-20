using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;
using ReMi.BusinessEntities.Auth;
using ReMi.Common.Cqrs;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Api.Insfrastructure.Notifications.Filters
{
    public class NotificationFilterApplying : INotificationFilterApplying
    {
        public IDependencyResolver DependencyResolver { get; set; }

        public bool Apply(IEvent evnt, Account account, List<Subscription> subscriptions)
        {
            var evntTypeName = evnt.GetType().Name;

            var affectedSubscriptions = subscriptions.Where(o => o.EventName.Equals(evntTypeName, StringComparison.InvariantCultureIgnoreCase));
            if (!affectedSubscriptions.Any())
                return false;

            var result =
                ResolveFilterAplications(evnt as INotificationFilter, account, subscriptions)
                & ResolveFilterAplications(evnt as INotificationFilterByProduct, account, subscriptions)
                & ResolveFilterAplications(evnt as INotificationFilterByReleaseTaskId, account, subscriptions)
                & ResolveFilterAplications(evnt as INotificationFilterByReleaseType, account, subscriptions)
                & ResolveFilterAplications(evnt as INotificationFilterByReleaseWindowId, account, subscriptions)
                & ResolveFilterAplications(evnt as INotificationFilterByReleaseWindowIdNotRequestor, account, subscriptions)
                & ResolveFilterAplications(evnt as INotificationFilterForUser, account, subscriptions)
                & ResolveFilterAplications(evnt as INotificationFilterByNotRequestor, account, subscriptions);

            return result;
        }

        private bool ResolveFilterAplications<T>(T filter, Account account, List<Subscription> subscriptions) where T : class, INotificationFilter
        {
            if (filter == null) return true;

            var filterApplication = DependencyResolver.GetService(typeof(INotificationFilterApplication<T>)) as INotificationFilterApplication<T>;
            if (filterApplication == null)
                return true;

            return filterApplication.Apply(filter, account, subscriptions);
        }
    }
}
