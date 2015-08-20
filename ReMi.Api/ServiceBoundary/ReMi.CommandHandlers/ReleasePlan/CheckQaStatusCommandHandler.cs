using System;
using System.Linq;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Commands.ReleaseCalendar;
using ReMi.Commands.ReleasePlan;
using ReMi.Common.Constants;
using ReMi.Common.Constants.ContinuousDelivery;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Utils.Enums;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.Events.ReleasePlan;
using ReMi.Queries.ContinuousDelivery;

namespace ReMi.CommandHandlers.ReleasePlan
{
    public class CheckQaStatusCommandHandler : IHandleCommand<CheckQaStatusCommand>
    {
        public IHandleQuery<GetContinuousDeliveryStatusRequest, GetContinuousDeliveryStatusResponse> StatusQuery
        {
            get;
            set;
        }

        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }

        public IPublishEvent EventPublisher { get; set; }

        public ICommandDispatcher CommandDispatcher { get; set; }

        public void Handle(CheckQaStatusCommand command)
        {
            ReleaseWindow window;
            using (var gateway = ReleaseWindowGatewayFactory())
            {
                window = gateway.GetByExternalId(command.ReleaseWindowId);
            }

            var status = StatusQuery.Handle(new GetContinuousDeliveryStatusRequest { Products = window.Products });

            EventPublisher.Publish(new QaStatusCheckedEvent
            {
                ReleaseWindowId = command.ReleaseWindowId,
                StatusCheckItems = status.StatusCheck
            });

            if (window.ApprovedOn == null)
            {
                return;
            }

            var currentDecision = status.StatusCheck.Any(s => s.Status != StatusType.Green)
                ? ReleaseDecision.NoGo
                : ReleaseDecision.Go;

            if (EnumDescriptionHelper.GetDescription(currentDecision) == window.ReleaseDecision)
            {
                return;
            }

            CommandDispatcher.Send(new UpdateReleaseDecisionCommand
            {
                ReleaseWindowId = command.ReleaseWindowId,
                ReleaseDecision = currentDecision
            });
        }
    }
}
