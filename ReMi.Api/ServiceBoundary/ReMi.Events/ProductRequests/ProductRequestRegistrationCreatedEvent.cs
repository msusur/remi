using System;
using ReMi.BusinessEntities.ProductRequests;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.ProductRequests
{
    public class ProductRequestRegistrationCreatedEvent : IEvent
    {
        public EventContext Context { get; set; }

        public ProductRequestRegistration Registration { get; set; }

        public override string ToString()
        {
            return String.Format("[Registration={0}, Context={1}]", Registration, Context);
        }
    }
}
