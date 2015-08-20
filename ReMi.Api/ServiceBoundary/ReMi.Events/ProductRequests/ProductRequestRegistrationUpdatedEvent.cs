using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.ProductRequests;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ProductRequests
{
    public class ProductRequestRegistrationUpdatedEvent : IEvent
    {
        public EventContext Context { get; set; }

        public ProductRequestRegistration Registration { get; set; }

        public IEnumerable<Guid> ChangedTasks { get; set; }

        public override string ToString()
        {
            return String.Format("[Registration={0}, ChangedTasks={1}, Context={2}]", Registration, ChangedTasks.FormatElements(), Context);
        }
    }
}
