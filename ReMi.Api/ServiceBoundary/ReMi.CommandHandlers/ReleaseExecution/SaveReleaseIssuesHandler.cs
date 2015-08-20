using System;
using ReMi.Commands.ReleaseCalendar;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.Events.ReleaseCalendar;

namespace ReMi.CommandHandlers.ReleaseExecution
{
    public class SaveReleaseIssuesHandler : IHandleCommand<SaveReleaseIssuesCommand>
    {
        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }
        public IPublishEvent EventPublisher { get; set; }

        public void Handle(SaveReleaseIssuesCommand command)
        {
            using (var gateway = ReleaseWindowGatewayFactory())
            {
                gateway.SaveIssues(command.ReleaseWindow);
            }

            EventPublisher.Publish(new ReleaseIssuesUpdatedEvent
            {
                ReleaseWindowId = command.ReleaseWindow.ExternalId,
                Issues = command.ReleaseWindow.Issues
            });
        }
    }
}
