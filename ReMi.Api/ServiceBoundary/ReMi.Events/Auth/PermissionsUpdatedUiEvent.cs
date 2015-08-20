using System;
using System.Collections.Generic;
using ReMi.Common.Utils;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Events.Auth
{
    public class PermissionsUpdatedUiEvent : IEvent, INotificationFilterByRole
    {
        public EventContext Context { get; set; }
        public Guid RoleId { get; set; }

        public IEnumerable<String> Commands { get; set; }
        public IEnumerable<String> Queries { get; set; }

        public override string ToString()
        {
            return String.Format("[Commands={0}, Queries={1}, RoleId={2}, Context={3}]", Commands.FormatElements(),
                Queries.FormatElements(), RoleId, Context);
        }
    }
}
