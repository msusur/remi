using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Common.Logging;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.Exceptions;
using ReMi.Commands.ReleaseCalendar;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Events.ReleaseCalendar;

namespace ReMi.CommandHandlers.ReleaseCalendar
{
    public class CancelReleaseWindowHandler : IHandleCommand<CancelReleaseWindowCommand>
    {
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();

        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }
        public Func<IReleaseParticipantGateway> ReleaseParticipantGatewayFactory { get; set; }
        public IPublishEvent PublishEvent { get; set; }
        public IMappingEngine Mapper { get; set; }

        public void Handle(CancelReleaseWindowCommand request)
        {
            CancelReleaseWindow(request);
        }

        private void CancelReleaseWindow(CancelReleaseWindowCommand request)
        {
            using (IReleaseWindowGateway releaseWindowGateway = ReleaseWindowGatewayFactory())
            {
                var releaseWindow = releaseWindowGateway.GetByExternalId(request.ExternalId);
                if (releaseWindow == null)
                {
                    throw new ReleaseNotFoundException(request.ExternalId);
                }

                List<Account> participants;
                using (var releaseParticipantGateway = ReleaseParticipantGatewayFactory())
                {
                    participants = releaseParticipantGateway.GetReleaseParticipants(releaseWindow.ExternalId).Select(o => o.Account).ToList();
                }

                Logger.DebugFormat("Canceling release with ExternalId={0}", releaseWindow.ExternalId);

                releaseWindowGateway.Cancel(releaseWindow);

                PublishEvent.Publish(new ReleaseWindowCanceledEvent
                {
                    Context = request.CommandContext.CreateChild<EventContext>(),
                    ReleaseWindow = releaseWindow,
                    Participants = participants
                });
            }
        }
    }
}
