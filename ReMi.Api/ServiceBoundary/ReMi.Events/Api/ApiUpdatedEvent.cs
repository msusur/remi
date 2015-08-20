using ReMi.BusinessEntities.Api;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Events;
using System;
using System.Collections.Generic;

namespace ReMi.Events.Api
{
    public class ApiUpdatedEvent: IEvent
    {
        public EventContext Context { get; set; }
        public List<ApiDescription> RemovedDescriptions { get; set; }
        public List<ApiDescription> AddedDescriptions { get; set; }

        public override string ToString()
        {
            return String.Format("[RemovedDescriptions={0}, AddedDescriptions={1}, Context={2}]",
                RemovedDescriptions.FormatElements(), AddedDescriptions.FormatElements(), Context);
        }
    }
}
