using System;
using ReMi.BusinessEntities.Products;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.Packages
{
    public class NewPackageAddedEvent : IEvent, INotificationFilterByNotRequestor
    {
        public EventContext Context { get; set; }

        public Guid RequestorId { get { return Context.UserId; } }

        public Product Package { get; set; }

        public override string ToString()
        {
            return String.Format("[Context={0}, Package={1}]", Context, Package);
        }
    }
}
