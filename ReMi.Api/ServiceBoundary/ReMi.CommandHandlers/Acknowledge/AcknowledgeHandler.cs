using System;
using ReMi.Commands.Acknowledge;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Events.ReleasePlan;

namespace ReMi.CommandHandlers.Acknowledge
{
    public class AcknowledgeHandler : IHandleCommand<ApproveReleaseParticipationCommand>, IHandleCommand<ClearReleaseAcknowledgesCommand>
    {
        public Func<IReleaseParticipantGateway> ReleaseParticipantGatewayFactory { get; set; }
        public IPublishEvent EventPublisher { get; set; }

        public void Handle(ApproveReleaseParticipationCommand command)
        {
            Guid releaseWindowGuid;
            using (var gateway = ReleaseParticipantGatewayFactory())
            {
                gateway.ApproveReleaseParticipation(command.ReleaseParticipantId);
                releaseWindowGuid = gateway.GetReleaseWindow(command.ReleaseParticipantId).ExternalId;
            }

            EventPublisher.Publish(new ReleaseParticipationConfirmedEvent
            {
                ReleaseParticipantId = command.ReleaseParticipantId,
                ReleaseWindowGuid = releaseWindowGuid
            });
        }

        public void Handle(ClearReleaseAcknowledgesCommand command)
        {
            using (var gateway = ReleaseParticipantGatewayFactory())
            {
                gateway.ClearParticipationApprovements(command.ReleaseWindowId,
                    command.CommandContext.UserId);
            }
        }
    }
}
