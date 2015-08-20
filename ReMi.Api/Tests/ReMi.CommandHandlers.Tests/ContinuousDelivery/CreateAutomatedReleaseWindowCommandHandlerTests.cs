using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ContinuousDelivery;
using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessEntities.Products;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.CommandHandlers.ContinuousDelivery;
using ReMi.Commands.ContinuousDelivery;
using ReMi.Commands.ReleaseCalendar;
using ReMi.Commands.ReleasePlan;
using ReMi.Common.Constants.ContinuousDelivery;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Utils;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.Events.ReleaseCalendar;
using ReMi.Events.ReleaseExecution;
using ReMi.Queries.ContinuousDelivery;
using ReMi.Queries.ReleasePlan;

namespace ReMi.CommandHandlers.Tests.ContinuousDelivery
{
    public class CreateAutomatedReleaseWindowCommandHandlerTests : TestClassFor<CreateAutomatedReleaseWindowCommandHandler>
    {
        private Mock<IHandleQuery<GetContinuousDeliveryStatusRequest, GetContinuousDeliveryStatusResponse>> _getContinuousDeliveryStatusMock;
        private Mock<IReleaseWindowGateway> _releaseWindowGatewayMock;
        private Mock<IProductGateway> _productGatewayMock;
        private Mock<ICommandDispatcher> _commandDispatcherMock;
        private Mock<IPublishEvent> _eventPublisherMock;

        private Mock<IHandleQuery<GetReleaseContentInformationRequest, GetReleaseContentInformationResponse>>
            _contentQueryMock;

        private DateTime _startTime;

        protected override CreateAutomatedReleaseWindowCommandHandler ConstructSystemUnderTest()
        {
            return new CreateAutomatedReleaseWindowCommandHandler
            {
                CommandDispatcher = _commandDispatcherMock.Object,
                ReleaseWindowGatewayFactory = () => _releaseWindowGatewayMock.Object,
                EventPublisher = _eventPublisherMock.Object,
                ProductGatewayFactory = () => _productGatewayMock.Object,
                GetContinuousDeliveryStatus = _getContinuousDeliveryStatusMock.Object,
                GetReleaseContentInformationQuery = _contentQueryMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _getContinuousDeliveryStatusMock = new Mock<IHandleQuery<GetContinuousDeliveryStatusRequest, GetContinuousDeliveryStatusResponse>>();
            _releaseWindowGatewayMock = new Mock<IReleaseWindowGateway>();
            _productGatewayMock = new Mock<IProductGateway>();
            _commandDispatcherMock = new Mock<ICommandDispatcher>();
            _eventPublisherMock = new Mock<IPublishEvent>();
            _contentQueryMock =
                new Mock<IHandleQuery<GetReleaseContentInformationRequest, GetReleaseContentInformationResponse>>();

            _startTime = DateTime.UtcNow;
            SystemTime.Mock(_startTime);

            base.TestInitialize();
        }

        [Test]
        [ExpectedException(typeof(ManualBookReleaseOfAutomatedException))]
        public void Handle_ShouldThrowException_WhenProductIsNotOnAutomatedReleaseTrack()
        {
            var command = BuildCommand();

            _productGatewayMock.Setup(x => x.GetProducts(It.IsAny<IEnumerable<string>>()))
                .Returns(new[] { new Product { ReleaseTrack = ReleaseTrack.Manual } });

            Sut.Handle(command);
        }

        [Test]
        public void Handle_ShouldNotThrowException_WhenColisionFoundWithReleaseForSameProductButAutomatedRelease()
        {
            var command = BuildCommand();

            _productGatewayMock.Setup(x => x.GetProduct(command.Product))
                .Returns(new Product { ReleaseTrack = ReleaseTrack.Automated });
            _releaseWindowGatewayMock.Setup((x => x.FindFirstOverlappedRelease(command.Product, _startTime, _startTime.AddHours(2))))
                .Returns(new ReleaseWindow { StartTime = _startTime.AddHours(1), ReleaseType = ReleaseType.Automated });
            _getContinuousDeliveryStatusMock.Setup(x => x.Handle(It.IsAny<GetContinuousDeliveryStatusRequest>()))
                .Returns(new GetContinuousDeliveryStatusResponse
                {
                    StatusCheck = new List<StatusCheckItem> { new StatusCheckItem { Status = StatusType.Green } }
                });

            _contentQueryMock.Setup(
                h => h.Handle(It.Is<GetReleaseContentInformationRequest>(x => x.ReleaseWindowId == command.ExternalId)))
                .Returns(new GetReleaseContentInformationResponse
                {
                    Content = new List<ReleaseContentTicket>
                    {
                        new ReleaseContentTicket
                        {
                            TicketDescription = "superticket"
                        }
                    }
                });

            Sut.Handle(command);
        }

        [Test]
        public void Handle_ShouldCreateReleaseWindowAndSendEvent_WhenQACheckSucced()
        {
            var command = BuildCommand();

            _productGatewayMock.Setup(x => x.GetProduct(command.Product))
                .Returns(new Product { ReleaseTrack = ReleaseTrack.Automated });
            _getContinuousDeliveryStatusMock.Setup(x => x.Handle(It.IsAny<GetContinuousDeliveryStatusRequest>()))
                .Returns(new GetContinuousDeliveryStatusResponse
                {
                    StatusCheck = new List<StatusCheckItem> { new StatusCheckItem { Status = StatusType.Green } }
                });

            _contentQueryMock.Setup(
               h => h.Handle(It.Is<GetReleaseContentInformationRequest>(x => x.ReleaseWindowId == command.ExternalId)))
               .Returns(new GetReleaseContentInformationResponse
               {
                   Content = new List<ReleaseContentTicket>
                    {
                        new ReleaseContentTicket
                        {
                            TicketDescription = "superticket"
                        }
                    }
               });

            Sut.Handle(command);

            _releaseWindowGatewayMock.Verify(x => x.Create(It.Is<ReleaseWindow>(
                rw => rw.ExternalId == command.ExternalId
                      && rw.Description == command.Description
                      && rw.Products.Any() && rw.Products.First() == command.Product
                      && rw.StartTime == _startTime
                      && rw.RequiresDowntime == false
                      && rw.OriginalStartTime == _startTime
                      && rw.ReleaseType == ReleaseType.Automated
                      && !string.IsNullOrEmpty(rw.Sprint)
                      ), command.CommandContext.UserId), Times.Once);

            _eventPublisherMock.Verify(x => x.Publish(It.Is<ReleaseWindowBookedEvent>(
                e => e.ReleaseWindow.ExternalId == command.ExternalId)), Times.Once);
        }

        [Test]
        [ExpectedException(typeof(NoGoDecisionException))]
        public void Handle_ShouldThrowNoGoException_WhenOneOfQACheckFail()
        {
            var command = BuildCommand();

            _productGatewayMock.Setup(x => x.GetProduct(command.Product))
                .Returns(new Product { ReleaseTrack = ReleaseTrack.Automated });
            _getContinuousDeliveryStatusMock.Setup(x => x.Handle(It.IsAny<GetContinuousDeliveryStatusRequest>()))
                .Returns(new GetContinuousDeliveryStatusResponse
                {
                    StatusCheck = new List<StatusCheckItem> {
                        new StatusCheckItem { Status = StatusType.Green },
                        new StatusCheckItem { Status = StatusType.Red }
                    },
                });

            Sut.Handle(command);
        }

        [Test]
        [ExpectedException(typeof(NoGoDecisionException))]
        public void Handle_ShouldThrowNoGoException_WhenFailedToCheckQAStatus()
        {
            var command = BuildCommand();

            _productGatewayMock.Setup(x => x.GetProduct(command.Product))
                .Returns(new Product { ReleaseTrack = ReleaseTrack.Automated });

            Sut.Handle(command);
        }

        [Test]
        public void Handle_ShouldCreateReleaseWindowAndCloseIt_WhenQAStatusIsFailed()
        {
            var command = BuildCommand();

            _productGatewayMock.Setup(x => x.GetProduct(command.Product))
                .Returns(new Product { ReleaseTrack = ReleaseTrack.Automated });
            _getContinuousDeliveryStatusMock.Setup(x => x.Handle(It.IsAny<GetContinuousDeliveryStatusRequest>()))
                .Returns(new GetContinuousDeliveryStatusResponse
                {
                    StatusCheck = new List<StatusCheckItem> { new StatusCheckItem { Status = StatusType.Red } }
                });

            try { Sut.Handle(command); }
            catch (NoGoDecisionException) { }

            _releaseWindowGatewayMock.Verify(x => x.Create(It.Is<ReleaseWindow>(
                rw => rw.ExternalId == command.ExternalId), command.CommandContext.UserId), Times.Once);

            _releaseWindowGatewayMock.Verify(x => x.CloseRelease(It.IsNotNull<string>(), command.ExternalId), Times.Once);

            _commandDispatcherMock.Verify(x => x.Send(It.Is<UpdateReleaseDecisionCommand>(
                c => c.ReleaseDecision == ReleaseDecision.NoGo
                    && c.ReleaseWindowId == command.ExternalId)), Times.Once);
        }

        [Test]
        public void Handle_ShouldChangeDecisionToGoAndApproveRelease_WhenAllConditionsPass()
        {
            var command = BuildCommand();

            _productGatewayMock.Setup(x => x.GetProduct(command.Product))
                .Returns(new Product { ReleaseTrack = ReleaseTrack.Automated });
            _getContinuousDeliveryStatusMock.Setup(x => x.Handle(It.IsAny<GetContinuousDeliveryStatusRequest>()))
                .Returns(new GetContinuousDeliveryStatusResponse
                {
                    StatusCheck = new List<StatusCheckItem> { new StatusCheckItem { Status = StatusType.Green } }
                });
            _contentQueryMock.Setup(
               h => h.Handle(It.Is<GetReleaseContentInformationRequest>(x => x.ReleaseWindowId == command.ExternalId)))
               .Returns(new GetReleaseContentInformationResponse
               {
                   Content = new List<ReleaseContentTicket>
                    {
                        new ReleaseContentTicket
                        {
                            TicketDescription = "superticket",
                            IncludeToReleaseNotes = true
                        }
                    }
               });

            Sut.Handle(command);


            _releaseWindowGatewayMock.Verify(x => x.Create(It.Is<ReleaseWindow>(
                rw => rw.ExternalId == command.ExternalId), command.CommandContext.UserId), Times.Once);

            _releaseWindowGatewayMock.Verify(x => x.ApproveRelease(command.ExternalId), Times.Once);

            _commandDispatcherMock.Verify(x => x.Send(It.Is<UpdateReleaseDecisionCommand>(
                c => c.ReleaseDecision == ReleaseDecision.Go
                    && c.ReleaseWindowId == command.ExternalId)), Times.Once);

            _eventPublisherMock.Verify(x => x.Publish(It.Is<ReleaseWindowApprovedEvent>(
                e => e.ReleaseWindow.ExternalId == command.ExternalId)), Times.Once);

            _eventPublisherMock.Verify(x => x.Publish(It.Is<ReleaseStatusChangedEvent>(
                e => e.ReleaseWindowId == command.ExternalId
                    && e.ReleaseStatus == "Approved")), Times.Once);

            _commandDispatcherMock.Verify(
                c =>
                    c.Send(
                        It.Is<PersistTicketsCommand>(
                            x => x.Tickets.Count() == 1 && x.Tickets.First().TicketDescription == "superticket")));
        }

        private static CreateAutomatedReleaseWindowCommand BuildCommand()
        {
            return Builder<CreateAutomatedReleaseWindowCommand>.CreateNew()
                .With(w => w.Product, RandomData.RandomString(10))
                .With(w => w.ExternalId, Guid.NewGuid())
                .With(w => w.Description, RandomData.RandomString(10))
                .With(w => w.CommandContext, Builder<CommandContext>.CreateNew()
                    .With(c => c.UserId, Guid.NewGuid())
                    .With(c => c.IsSynchronous, true)
                    .Build())
                .Build();
        }
    }
}
