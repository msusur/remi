using System;
using System.Linq;
using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Commands.ContinuousDelivery;
using ReMi.Commands.Metrics;
using ReMi.Commands.ReleaseCalendar;
using ReMi.Commands.ReleasePlan;
using ReMi.Common.Constants.ContinuousDelivery;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Constants.ReleasePlan;
using ReMi.Common.Utils;
using ReMi.Common.Utils.Enums;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Contracts.Plugins.Data.Authentication;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.Events.ReleaseCalendar;
using ReMi.Events.ReleaseExecution;
using ReMi.Queries.ContinuousDelivery;
using ReMi.Queries.ReleasePlan;

namespace ReMi.CommandHandlers.ContinuousDelivery
{
    public class CreateAutomatedReleaseWindowCommandHandler : IHandleCommand<CreateAutomatedReleaseWindowCommand>
    {
        public IHandleQuery<GetContinuousDeliveryStatusRequest, GetContinuousDeliveryStatusResponse> GetContinuousDeliveryStatus { get; set; }
        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }
        public Func<IProductGateway> ProductGatewayFactory { get; set; }
        public ICommandDispatcher CommandDispatcher { get; set; }
        public IPublishEvent EventPublisher { get; set; }
        public IHandleQuery<GetReleaseContentInformationRequest, GetReleaseContentInformationResponse> GetReleaseContentInformationQuery { get; set; }

        public void Handle(CreateAutomatedReleaseWindowCommand request)
        {
            using (var releaseWindowGateway = ReleaseWindowGatewayFactory())
            {
                var releaseWindow = GetReleaseWindow(request);

                CheckIfProductOnAutomatedTrack(releaseWindow);

                CreateNewReleaseWindow(releaseWindowGateway, releaseWindow, request.CommandContext.UserId);

                CheckQaStatus(releaseWindowGateway, request, releaseWindow);

                CommandDispatcher.Send(new CreateReleaseMetricsCommand
                {
                    ReleaseWindow = releaseWindow,
                    CommandContext = request.CommandContext.CreateChild()
                });

                ApproveRelease(releaseWindow);
            }
        }

        private void CheckIfProductOnAutomatedTrack(ReleaseWindow releaseWindow)
        {
            using (var productGateway = ProductGatewayFactory())
            {
                var nonAutomatedProducts = productGateway.GetProducts(releaseWindow.Products)
                    .Where(x => x.ReleaseTrack != ReleaseTrack.Automated)
                    .ToList();

                if (nonAutomatedProducts.Any())
                {
                    throw new ManualBookReleaseOfAutomatedException(nonAutomatedProducts.First().Description);
                }
            }
        }

        private static ReleaseWindow GetReleaseWindow(CreateAutomatedReleaseWindowCommand request)
        {
            var startTime = SystemTime.Now;
            return new ReleaseWindow
            {
                ReleaseType = ReleaseType.Automated,
                RequiresDowntime = false,
                Description = request.Description,
                StartTime = startTime,
                Sprint = string.Format("Automated ({0:yy-MM-dd})", startTime),
                OriginalStartTime = startTime,
                Products = new[] { request.Product },
                ExternalId = request.ExternalId
            };
        }

        private void CheckQaStatus(IReleaseWindowGateway releaseWindowGateway,
            CreateAutomatedReleaseWindowCommand request, ReleaseWindow releaseWindow)
        {
            var qaCheck = GetContinuousDeliveryStatus.Handle(new GetContinuousDeliveryStatusRequest
            {
                Products = new[] { request.Product }
            });

            if (qaCheck != null && !qaCheck.StatusCheck.IsNullOrEmpty() && qaCheck.StatusCheck.All(x => x.Status != StatusType.Red))
                return;

            var releaseNotes = qaCheck == null
                ? "Cannot check release QA status"
                : qaCheck.StatusCheck.FormatElements();
            releaseWindowGateway.CloseRelease(releaseNotes, releaseWindow.ExternalId);
            CommandDispatcher.Send(new UpdateReleaseDecisionCommand
            {
                ReleaseDecision = ReleaseDecision.NoGo,
                ReleaseWindowId = releaseWindow.ExternalId
            });
            throw new NoGoDecisionException(request.Product);
        }

        private void CreateNewReleaseWindow(IReleaseWindowGateway releaseWindowGateway, ReleaseWindow releaseWindow, Guid creatorId)
        {
            releaseWindowGateway.Create(releaseWindow, creatorId);

            EventPublisher.Publish(new ReleaseWindowBookedEvent { ReleaseWindow = releaseWindow });
        }

        private void ApproveRelease(ReleaseWindow releaseWindow)
        {
            using (var gateway = ReleaseWindowGatewayFactory())
            {
                gateway.ApproveRelease(releaseWindow.ExternalId);

                CommandDispatcher.Send(new UpdateReleaseDecisionCommand
                {
                    ReleaseDecision = ReleaseDecision.Go,
                    ReleaseWindowId = releaseWindow.ExternalId
                });

                EventPublisher.Publish(new ReleaseWindowApprovedEvent { ReleaseWindow = releaseWindow });
                EventPublisher.Publish(new ReleaseStatusChangedEvent
                {
                    ReleaseWindowId = releaseWindow.ExternalId,
                    ReleaseStatus = EnumDescriptionHelper.GetDescription(ReleaseStatus.Approved)
                });

                var content =
                    GetReleaseContentInformationQuery.Handle(new GetReleaseContentInformationRequest
                    {
                        ReleaseWindowId = releaseWindow.ExternalId
                    })
                        .Content.Where(c => c.IncludeToReleaseNotes)
                        .ToList();

                CommandDispatcher.Send(new PersistTicketsCommand
                {
                    Tickets = content,
                    ReleaseWindowId = releaseWindow.ExternalId
                });
            }
        }
    }
}
