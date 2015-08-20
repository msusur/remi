using System;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessLogic.ReleasePlan;
using ReMi.CommandHandlers.ReleaseCalendar;
using ReMi.Commands.Metrics;
using ReMi.Commands.ReleaseCalendar;
using ReMi.Commands.ReleasePlan;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.Events.ReleaseCalendar;

namespace ReMi.CommandHandlers.Tests.ReleaseCalendar
{
    public class BookReleaseWindowHandlerTests : TestClassFor<BookReleaseWindowHandler>
    {
        private Mock<IReleaseWindowGateway> _releaseWindowGatewayMock;
        private Mock<ICommandDispatcher> _commandDispatcherMock;
        private Mock<IPublishEvent> _eventPublisherMock;
        private Mock<IReleaseWindowOverlappingChecker> _releaseWindowOverlappingCheckerMock;
        private Mock<IReleaseWindowHelper> _releaseWindowHelperMock;

        protected override BookReleaseWindowHandler ConstructSystemUnderTest()
        {
            return new BookReleaseWindowHandler
            {
                CommandDispatcher = _commandDispatcherMock.Object,
                ReleaseWindowGatewayFactory = () => _releaseWindowGatewayMock.Object,
                EventPublisher = _eventPublisherMock.Object,
                ReleaseWindowOverlappingChecker = _releaseWindowOverlappingCheckerMock.Object,
                ReleaseWindowHelper = _releaseWindowHelperMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseWindowGatewayMock = new Mock<IReleaseWindowGateway>();
            _commandDispatcherMock = new Mock<ICommandDispatcher>();
            _eventPublisherMock = new Mock<IPublishEvent>();
            _releaseWindowOverlappingCheckerMock = new Mock<IReleaseWindowOverlappingChecker>();
            _releaseWindowHelperMock = new Mock<IReleaseWindowHelper>();
            _releaseWindowHelperMock.Setup(x => x.IsMaintenance(It.IsAny<ReleaseWindow>())).Returns(false);

            base.TestInitialize();
        }

        [Test]
        [ExpectedException(typeof(ReleaseAlreadyBookedException))]
        public void Handle_ShouldThrowReleaseAlreadyBookedException_WhenNewReleaseOverlaps()
        {
            var product = RandomData.RandomString(1, 10);

            var bookedRelease = Builder<ReleaseWindow>.CreateNew()
                        .With(r => r.StartTime, RandomData.RandomDateTime())
                        .With(r => r.Products, new[] { product })
                        .Build();

            var command = Builder<BookReleaseWindowCommand>.CreateNew()
                .With(w => w.ReleaseWindow,
                    Builder<ReleaseWindow>.CreateNew()
                        .With(r => r.StartTime, bookedRelease.StartTime.AddMinutes(-90))
                        .With(r => r.Products, new[] { product })
                        .Build())
                .With(w => w.CommandContext = new CommandContext { UserId = Guid.NewGuid() })
                .Build();

            _releaseWindowOverlappingCheckerMock.Setup(o => o.FindOverlappedWindow(command.ReleaseWindow))
                .Returns(bookedRelease);

            Sut.Handle(command);
        }

        [Test]
        [ExpectedException(typeof(ApplicationException))]
        public void Handle_ShouldThrowException_WhenNonMaintananceReleaseHasMultipleProducts()
        {
            var bookedRelease = Builder<ReleaseWindow>.CreateNew()
                        .With(r => r.StartTime, RandomData.RandomDateTime())
                        .With(r => r.Products, new[] { "P1", "P2" })
                        .With(r => r.ReleaseType = ReleaseType.Scheduled)
                        .Build();

            var command = Builder<BookReleaseWindowCommand>.CreateNew()
                .With(w => w.ReleaseWindow, bookedRelease)
                .With(w => w.CommandContext = new CommandContext { UserId = Guid.NewGuid() })
                .Build();

            Sut.Handle(command);
        }

        [Test]
        public void Handle_ShouldNotThrowReleaseAlreadyBookedException_WhenNewReleaseOverlapsButTypeSystemMaintenance()
        {
            var bookedRelease = Builder<ReleaseWindow>.CreateNew()
                        .With(r => r.StartTime, RandomData.RandomDateTime())
                        .With(r => r.ReleaseType, ReleaseType.SystemMaintenance)
                        .With(r => r.Products, new[] { RandomData.RandomString(1, 20) })
                        .Build();

            var command = Builder<BookReleaseWindowCommand>.CreateNew()
                .With(w => w.ReleaseWindow,
                    Builder<ReleaseWindow>.CreateNew()
                        .With(r => r.StartTime, bookedRelease.StartTime)
                        .With(r => r.Products, bookedRelease.Products)
                        .With(r => r.ReleaseType, ReleaseType.SystemMaintenance)
                        .Build())
                .With(w => w.CommandContext = new CommandContext { UserId = Guid.NewGuid() })
                .Build();

            _releaseWindowOverlappingCheckerMock.Setup(o => o.FindOverlappedWindow(command.ReleaseWindow))
                .Returns((ReleaseWindow)null);

            Sut.Handle(command);

            _releaseWindowGatewayMock.Verify(g => g.Create(command.ReleaseWindow, command.CommandContext.UserId));
        }

        [Test]
        public void Handle_ShouldCreateReleaseWindow_WhenThereAreNoCollidingReleases()
        {
            var command = Builder<BookReleaseWindowCommand>.CreateNew()
                .With(w => w.ReleaseWindow = Builder<ReleaseWindow>.CreateNew().With(x => x.Products, new[] { "P1" }).Build())
                .With(w => w.CommandContext = new CommandContext { UserId = Guid.NewGuid() })
                .Build();

            _releaseWindowOverlappingCheckerMock.Setup(o => o.FindOverlappedWindow(command.ReleaseWindow))
                .Returns((ReleaseWindow)null);

            Sut.Handle(command);

            _releaseWindowGatewayMock.Verify(g => g.Create(command.ReleaseWindow, command.CommandContext.UserId));
        }

        [Test]
        public void Handle_ShouldPublishWindowBookedEvent_WhenThereAreNoCollidingReleases()
        {
            var product = RandomData.RandomString(5);

            var command = Builder<BookReleaseWindowCommand>.CreateNew()
                .With(
                    w =>
                        w.ReleaseWindow =
                            Builder<ReleaseWindow>.CreateNew()
                                .With(x => x.ExternalId, Guid.NewGuid())
                                .With(x => x.Products, new[] { product })
                                .Build())
                .With(w => w.CommandContext = new CommandContext { UserId = Guid.NewGuid() })
                .Build();

            _releaseWindowOverlappingCheckerMock.Setup(o => o.FindOverlappedWindow(command.ReleaseWindow))
                .Returns((ReleaseWindow)null);

            Sut.Handle(command);

            _eventPublisherMock.Verify(
                g =>
                    g.Publish(
                        It.Is<ReleaseWindowBookedEvent>(
                            x =>
                                x.ReleaseWindow.ExternalId == command.ReleaseWindow.ExternalId &&
                                x.ReleaseWindow.Products.First() == product)));
        }

        [Test]
        public void Handle_ShouldpopulateOriginalStartDateWithStartDateValue_WhenInvoked()
        {
            var command = Builder<BookReleaseWindowCommand>.CreateNew()
                .With(w => w.ReleaseWindow = Builder<ReleaseWindow>.CreateNew()
                    .With(o => o.StartTime, RandomData.RandomDate())
                    .With(o => o.OriginalStartTime, DateTime.MinValue)
                    .With(x => x.Products, new[] { "P1" })
                    .Build())
                .With(w => w.CommandContext = new CommandContext { UserId = Guid.NewGuid() })
                .Build();

            _releaseWindowOverlappingCheckerMock.Setup(o => o.FindOverlappedWindow(command.ReleaseWindow))
                .Returns((ReleaseWindow)null);

            Sut.Handle(command);

            _releaseWindowGatewayMock.Verify(g => g.Create(It.Is<ReleaseWindow>(r => r.StartTime == r.OriginalStartTime), command.CommandContext.UserId));
        }

        [Test]
        public void Handle_ShouldSendCreateCheckListCommand_WhenInvoked()
        {
            var command = Builder<BookReleaseWindowCommand>.CreateNew()
                .With(w => w.ReleaseWindow,
                    Builder<ReleaseWindow>.CreateNew()
                        .With(o => o.StartTime, RandomData.RandomDate())
                        .With(o => o.OriginalStartTime, DateTime.MinValue)
                        .With(o => o.ExternalId, Guid.NewGuid())
                        .With(x => x.Products, new[] { "P1" })
                        .Build())
                .With(w => w.CommandContext = new CommandContext { UserId = Guid.NewGuid() })
                .Build();

            _releaseWindowOverlappingCheckerMock.Setup(o => o.FindOverlappedWindow(command.ReleaseWindow))
                .Returns((ReleaseWindow)null);

            Sut.Handle(command);

            _commandDispatcherMock.Verify(
                cd =>
                    cd.Send(It.Is<CreateCheckListCommand>(x => x.ReleaseWindowId == command.ReleaseWindow.ExternalId)));
        }

        [Test]
        public void Handle_ShouldSendCreateMetricsCommand_WhenReleaseIsScheduled()
        {
            var command = Builder<BookReleaseWindowCommand>.CreateNew()
                .With(w => w.ReleaseWindow,
                    Builder<ReleaseWindow>.CreateNew()
                        .With(o => o.StartTime, RandomData.RandomDate())
                        .With(o => o.OriginalStartTime, DateTime.MinValue)
                        .With(o => o.ExternalId, Guid.NewGuid())
                        .With(o => o.ReleaseType, ReleaseType.Scheduled)
                        .With(o => o.RequiresDowntime, true)
                        .With(x => x.Products, new[] { "P1" })
                        .Build())
                .With(w => w.CommandContext = new CommandContext { UserId = Guid.NewGuid() })
                .Build();

            _releaseWindowOverlappingCheckerMock.Setup(o => o.FindOverlappedWindow(command.ReleaseWindow))
                .Returns((ReleaseWindow)null);

            Sut.Handle(command);

            _commandDispatcherMock.Verify(
                cd =>
                    cd.Send(
                        It.Is<CreateReleaseMetricsCommand>(
                            x => x.ReleaseWindow == command.ReleaseWindow)));
        }
    }
}
