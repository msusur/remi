using System;
using System.Linq;
using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessLogic.ReleasePlan;
using ReMi.Commands.Metrics;
using ReMi.Commands.ReleaseCalendar;
using ReMi.Commands.ReleasePlan;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.Events.ReleaseCalendar;

namespace ReMi.CommandHandlers.ReleaseCalendar
{
    public class BookReleaseWindowHandler : IHandleCommand<BookReleaseWindowCommand>
    {
        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }
        public ICommandDispatcher CommandDispatcher { get; set; }
        public IPublishEvent EventPublisher { get; set; }
        public IReleaseWindowOverlappingChecker ReleaseWindowOverlappingChecker { get; set; }
        public IReleaseWindowHelper ReleaseWindowHelper { get; set; }
        
        public void Handle(BookReleaseWindowCommand command)
        {
            var releaseWindow = command.ReleaseWindow;

            if (releaseWindow.Products.Count() != 1 && !ReleaseWindowHelper.IsMaintenance(releaseWindow))
            {
                throw new ApplicationException("Only maintenance windows can have multiple products");
            }

            using (var releaseWindowGateway = ReleaseWindowGatewayFactory())
            {
                var overlapped = ReleaseWindowOverlappingChecker.FindOverlappedWindow(command.ReleaseWindow);
                if (overlapped != null)
                {
                    throw new ReleaseAlreadyBookedException(overlapped, releaseWindow.StartTime);
                }

                releaseWindow.OriginalStartTime = releaseWindow.StartTime;

                releaseWindowGateway.Create(releaseWindow, command.CommandContext.UserId);

                EventPublisher.Publish(new ReleaseWindowBookedEvent { ReleaseWindow = releaseWindow });

                CommandDispatcher.Send(new CreateCheckListCommand { ReleaseWindowId = releaseWindow.ExternalId });

                CommandDispatcher.Send(new CreateReleaseMetricsCommand { ReleaseWindow = releaseWindow });
            }
        }
    }
}
