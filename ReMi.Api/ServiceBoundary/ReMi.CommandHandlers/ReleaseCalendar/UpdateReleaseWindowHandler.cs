using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessLogic.ReleasePlan;
using ReMi.Commands.Acknowledge;
using ReMi.Commands.Metrics;
using ReMi.Commands.ReleaseCalendar;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.Events.ReleaseCalendar;
using System;
using System.Linq;

namespace ReMi.CommandHandlers.ReleaseCalendar
{
    public class UpdateReleaseWindowHandler : IHandleCommand<UpdateReleaseWindowCommand>
    {
        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }
        public ICommandDispatcher CommandDispatcher { get; set; }
        public IPublishEvent EventPublisher { get; set; }
        public IReleaseWindowOverlappingChecker ReleaseWindowOverlappingChecker { get; set; }
        public IReleaseWindowHelper ReleaseWindowHelper { get; set; }

        public void Handle(UpdateReleaseWindowCommand command)
        {
            if (command.ReleaseWindow.Products.Count() != 1 && !ReleaseWindowHelper.IsMaintenance(command.ReleaseWindow))
            {
                throw new ApplicationException("Only maintenance windows can have multiple products");
            }

            ReleaseWindow release;
            using (var releaseWindowGateway = ReleaseWindowGatewayFactory())
            {
                //check is exists
                release = releaseWindowGateway.GetByExternalId(command.ReleaseWindow.ExternalId);
                if (release == null)
                {
                    throw new ReleaseNotFoundException(command.ReleaseWindow);
                }
                if (release.ReleaseType == ReleaseType.Automated)
                {
                    throw new ManualBookReleaseOfAutomatedException(release.Products);
                }

                var overlapped = ReleaseWindowOverlappingChecker.FindOverlappedWindow(command.ReleaseWindow);
                if (overlapped != null)
                {
                    throw new ReleaseAlreadyBookedException(overlapped, release.StartTime);
                }

                releaseWindowGateway.Update(command.ReleaseWindow, command.AllowUpdateInPast);
            }

            if (release.RequiresDowntime != command.ReleaseWindow.RequiresDowntime
                || release.ReleaseType != command.ReleaseWindow.ReleaseType
                || release.StartTime != command.ReleaseWindow.StartTime
                || release.EndTime != command.ReleaseWindow.EndTime
                || !release.Products.OrderBy(x => x).SequenceEqual(command.ReleaseWindow.Products.OrderBy(x => x)))
            {
                CommandDispatcher.Send(new UpdateReleaseMetricsCommand
                {
                    CommandContext = command.CommandContext.CreateChild<CommandContext>(),
                    ReleaseWindow = command.ReleaseWindow
                });
                CommandDispatcher.Send(new ClearApproversSignaturesCommand
                {
                    CommandContext = command.CommandContext.CreateChild<CommandContext>(),
                    ReleaseWindowId = command.ReleaseWindow.ExternalId
                });

                CommandDispatcher.Send(new ClearReleaseAcknowledgesCommand
                {
                    CommandContext = command.CommandContext.CreateChild<CommandContext>(),
                    ReleaseWindowId = command.ReleaseWindow.ExternalId
                });
                CommandDispatcher.Send(new UpdateReleaseDecisionCommand
                {
                    CommandContext = command.CommandContext.CreateChild<CommandContext>(),
                    ReleaseDecision = ReleaseDecision.NoGo,
                    ReleaseWindowId = command.ReleaseWindow.ExternalId
                });
                CommandDispatcher.Send(new ClearReleaseContentCommand
                {
                    ReleaseWindowId = command.ReleaseWindow.ExternalId,
                    CommandContext = command.CommandContext.CreateChild()
                });
                CommandDispatcher.Send(new ClearReleaseChangesCommand
                {
                    ReleaseWindowId = command.ReleaseWindow.ExternalId,
                    CommandContext = command.CommandContext.CreateChild()
                });
            }

            EventPublisher.Publish(new ReleaseWindowUpdatedEvent
            {
                Context = command.CommandContext.CreateChild<EventContext>(),
                ReleaseWindow = command.ReleaseWindow
            });
        }
    }
}
