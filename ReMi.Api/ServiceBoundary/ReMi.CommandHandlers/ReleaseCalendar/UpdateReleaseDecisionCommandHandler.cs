using System;
using ReMi.Commands.ReleaseCalendar;
using ReMi.Common.Constants;
using ReMi.Common.Utils.Enums;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.Events.ReleaseExecution;

namespace ReMi.CommandHandlers.ReleaseCalendar
{
    public class UpdateReleaseDecisionCommandHandler : IHandleCommand<UpdateReleaseDecisionCommand>
    {
        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }
        public IPublishEvent PublishEvent { get; set; }

        public void Handle(UpdateReleaseDecisionCommand command)
        {
            using (var gateway = ReleaseWindowGatewayFactory())
            {
                gateway.UpdateReleaseDecision(command.ReleaseWindowId, command.ReleaseDecision);
            }
            PublishEvent.Publish(new ReleaseDecisionChangedEvent
            {
                ReleaseDecision = EnumDescriptionHelper.GetDescription(command.ReleaseDecision),
                ReleaseWindowId = command.ReleaseWindowId
            });
        }
    }
}
