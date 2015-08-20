using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessLogic.ReleasePlan;
using ReMi.CommandHandlers.ReleaseCalendar;
using ReMi.Commands.Acknowledge;
using ReMi.Commands.Metrics;
using ReMi.Commands.ReleaseCalendar;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.Events.ReleaseCalendar;
using System;
using System.Threading.Tasks;

namespace ReMi.CommandHandlers.Tests.ReleaseCalendar
{
    public class UpdateReleaseWindowHandlerTests : TestClassFor<UpdateReleaseWindowHandler>
    {
        private Mock<IReleaseWindowGateway> _releaseWindowGatewayMock;
        private Mock<IPublishEvent> _eventPublisherMock;
        private Mock<ICommandDispatcher> _commandDispatcherMock;
        private Mock<IReleaseWindowOverlappingChecker> _releaseWindowOverlappingCheckerMock;
        private Mock<IReleaseWindowHelper> _releaseWindowHelperMock;

        protected override void TestInitialize()
        {
            _releaseWindowGatewayMock = new Mock<IReleaseWindowGateway>(MockBehavior.Strict);
            _eventPublisherMock = new Mock<IPublishEvent>(MockBehavior.Strict);
            _commandDispatcherMock = new Mock<ICommandDispatcher>(MockBehavior.Strict);
            _releaseWindowOverlappingCheckerMock = new Mock<IReleaseWindowOverlappingChecker>(MockBehavior.Strict);
            _releaseWindowHelperMock = new Mock<IReleaseWindowHelper>(MockBehavior.Strict);
            _releaseWindowHelperMock.Setup(x => x.IsMaintenance(It.IsAny<ReleaseWindow>())).Returns(false);

            base.TestInitialize();
        }

        protected override UpdateReleaseWindowHandler ConstructSystemUnderTest()
        {
            return new UpdateReleaseWindowHandler
            {
                EventPublisher = _eventPublisherMock.Object,
                CommandDispatcher = _commandDispatcherMock.Object,
                ReleaseWindowGatewayFactory = () => _releaseWindowGatewayMock.Object,
                ReleaseWindowOverlappingChecker = _releaseWindowOverlappingCheckerMock.Object,
                ReleaseWindowHelper = _releaseWindowHelperMock.Object
            };
        }

        [Test]
        [ExpectedException(typeof(ReleaseNotFoundException))]
        public void Handle_ShouldThrowReleaseNotFoundException_WhenReleaseDoestExists()
        {
            var release = Builder<ReleaseWindow>.CreateNew()
                .With(r => r.ExternalId = Guid.NewGuid())
                .With(x => x.Products, new[] { "P1" })
                .Build();

            _releaseWindowGatewayMock.Setup(x => x.Dispose());
            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(release.ExternalId, false, false))
                .Returns((ReleaseWindow) null);

            Sut.Handle(new UpdateReleaseWindowCommand { ReleaseWindow = release });
        }

        [Test]
        [ExpectedException(typeof(ManualBookReleaseOfAutomatedException))]
        public void MoveReleaseWindow_ShouldThrowManualBookReleaseOfAutomatedException_WhenReleaseWasCreatedAsAutomated()
        {
            var release = Builder<ReleaseWindow>.CreateNew()
                .With(r => r.ExternalId = Guid.NewGuid())
                .With(r => r.ReleaseType, ReleaseType.Automated)
                .With(x => x.Products, new[] { "P1" })
                .Build();

            _releaseWindowGatewayMock.Setup(x => x.Dispose());
            _releaseWindowGatewayMock.Setup(g => g.GetByExternalId(release.ExternalId, false, It.IsAny<bool>()))
                .Returns(release);

            Sut.Handle(new UpdateReleaseWindowCommand { ReleaseWindow = release });
        }

        [Test]
        [ExpectedException(typeof(ReleaseAlreadyBookedException))]
        public void Handle_ShouldThrowReleaseAlreadyBookedException_WhenOverlappedReleaseExists()
        {
            var release = Builder<ReleaseWindow>.CreateNew()
                .With(r => r.ExternalId, Guid.NewGuid())
                .With(x => x.Products, new[] { "P1" })
                .Build();

            _releaseWindowGatewayMock.Setup(g => g.GetByExternalId(release.ExternalId, false, It.IsAny<bool>()))
                .Returns(release);

            var existingRelease = Builder<ReleaseWindow>.CreateNew()
               .With(r => r.ExternalId, Guid.NewGuid())
               .Build();

            _releaseWindowOverlappingCheckerMock.Setup(o => o.FindOverlappedWindow(release))
                .Returns(existingRelease);
            _releaseWindowGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(new UpdateReleaseWindowCommand { ReleaseWindow = release, CommandContext = ConstructContext() });
        }

        [Test]
        public void Handle_ShouldThrowException_WhenNonMaintenanceReleaseHasMultipleProducts()
        {
            var release = Builder<ReleaseWindow>.CreateNew()
                .With(r => r.ExternalId = Guid.NewGuid())
                .With(x => x.Products, new[] { "P1", "P2" })
                .With(r => r.ReleaseType = ReleaseType.Scheduled)
                .Build();

            var ex = Assert.Throws<ApplicationException>(
                () => Sut.Handle(new UpdateReleaseWindowCommand { ReleaseWindow = release, CommandContext = ConstructContext() }));

            Assert.AreEqual("Only maintenance windows can have multiple products", ex.Message);
        }

        [Test]
        public void Handle_ShouldResetRelease_WhenReleaseTypeHasChanged()
        {
            var existingRelease = Builder<ReleaseWindow>.CreateNew()
                .With(r => r.ExternalId = Guid.NewGuid())
                .With(r => r.ReleaseType, ReleaseType.ChangeRequest)
                .With(x => x.Products, new[] { "P1" })
                .Build();

            var release = Builder<ReleaseWindow>.CreateNew()
                .With(r => r.ExternalId = existingRelease.ExternalId)
                .With(r => r.ReleaseType, ReleaseType.Hotfix)
                .With(x => x.Products, new[] { "P1" })
                .Build();
            Test_ShouldResetRelease(release, existingRelease, Times.Once());
        }

        [Test]
        public void Handle_ShouldResetRelease_WhenReleaseRequiresDowntimeFlagHasChanged()
        {
            var existingRelease = Builder<ReleaseWindow>.CreateNew()
                .With(r => r.ExternalId = Guid.NewGuid())
                .With(r => r.ReleaseType, ReleaseType.ChangeRequest)
                .With(x => x.Products, new[] { "P1" })
                .Build();

            var release = Builder<ReleaseWindow>.CreateNew()
                .With(r => r.ExternalId = existingRelease.ExternalId)
                .With(r => r.ReleaseType, ReleaseType.Hotfix)
                .With(x => x.Products, new[] { "P1" })
                .Build();
            Test_ShouldResetRelease(release, existingRelease, Times.Once());
        }

        [Test]
        public void Handle_ShouldResetRelease_WhenReleaseStartTimeHasChanged()
        {
            var existingRelease = Builder<ReleaseWindow>.CreateNew()
                .With(r => r.ExternalId = Guid.NewGuid())
                .With(r => r.StartTime, RandomData.RandomDateTime(10000000, 20000000))
                .With(r => r.ReleaseType, ReleaseType.Hotfix)
                .With(x => x.Products, new[] { "P1" })
                .Build();

            var release = Builder<ReleaseWindow>.CreateNew()
                .With(r => r.ExternalId = existingRelease.ExternalId)
                .With(r => r.StartTime, RandomData.RandomDateTime(30000000, 40000000))
                .With(r => r.ReleaseType, ReleaseType.Hotfix)
                .With(x => x.Products, new[] { "P1" })
                .Build();
            Test_ShouldResetRelease(release, existingRelease, Times.Once());
        }

        [Test]
        public void Handle_ShouldResetRelease_WhenReleaseEndTimeHasChanged()
        {
            var existingRelease = Builder<ReleaseWindow>.CreateNew()
                .With(r => r.ExternalId = Guid.NewGuid())
                .With(r => r.EndTime, RandomData.RandomDateTime(10000000, 20000000))
                .With(r => r.ReleaseType, ReleaseType.Hotfix)
                .With(x => x.Products, new[] { "P1" })
                .Build();

            var release = Builder<ReleaseWindow>.CreateNew()
                .With(r => r.ExternalId = existingRelease.ExternalId)
                .With(r => r.EndTime, RandomData.RandomDateTime(30000000, 40000000))
                .With(r => r.ReleaseType, ReleaseType.Hotfix)
                .With(x => x.Products, new[] { "P1" })
                .Build();
            Test_ShouldResetRelease(release, existingRelease, Times.Once());
        }

        [Test]
        public void Handle_ShouldResetRelease_WhenProductListHasChanged()
        {
            var existingRelease = Builder<ReleaseWindow>.CreateNew()
                .With(r => r.ExternalId = Guid.NewGuid())
                .With(r => r.ReleaseType, ReleaseType.Hotfix)
                .With(x => x.Products, new[] { "P1" })
                .Build();

            var release = Builder<ReleaseWindow>.CreateNew()
                .With(r => r.ExternalId = existingRelease.ExternalId)
                .With(r => r.ReleaseType, ReleaseType.Hotfix)
                .With(x => x.Products, new[] { "P2" })
                .Build();
            Test_ShouldResetRelease(release, existingRelease, Times.Once());
        }

        [Test]
        public void Handle_ShouldNotResetRelease_WhenOnlyDescriptionOrSprintHasChanged()
        {
            var existingRelease = Builder<ReleaseWindow>.CreateNew()
                .With(r => r.ExternalId = Guid.NewGuid())
                .With(r => r.Sprint, RandomData.RandomString(5))
                .With(r => r.Description, RandomData.RandomString(5))
                .With(x => x.Products, new[] { "P1" })
                .Build();

            var release = Builder<ReleaseWindow>.CreateNew()
                .With(r => r.ExternalId = existingRelease.ExternalId)
                .With(r => r.Sprint, RandomData.RandomString(15))
                .With(r => r.Description, RandomData.RandomString(15))
                .With(x => x.Products, new[] { "P1" })
                .Build();
            Test_ShouldResetRelease(release, existingRelease, Times.Never());
        }

        private void Test_ShouldResetRelease(ReleaseWindow release, ReleaseWindow existingRelease, Times resetTimes)
        {
            var command = new UpdateReleaseWindowCommand {ReleaseWindow = release, CommandContext = ConstructContext()};

            _commandDispatcherMock.Setup(cd => cd.Send(It.Is<ClearReleaseAcknowledgesCommand>(
                c => c.ReleaseWindowId == release.ExternalId && c.CommandContext.ParentId == command.CommandContext.Id)))
                .Returns((Task) null);
            _commandDispatcherMock.Setup(cd => cd.Send(It.Is<ClearApproversSignaturesCommand>(
                c => c.ReleaseWindowId == release.ExternalId && c.CommandContext.ParentId == command.CommandContext.Id)))
                .Returns((Task) null);
            _commandDispatcherMock.Setup(cd => cd.Send(It.Is<UpdateReleaseMetricsCommand>(
                c => c.ReleaseWindow == release && c.CommandContext.ParentId == command.CommandContext.Id)))
                .Returns((Task) null);
            _commandDispatcherMock.Setup(cd => cd.Send(It.Is<UpdateReleaseDecisionCommand>(
                c => c.ReleaseWindowId == release.ExternalId
                    && c.ReleaseDecision == ReleaseDecision.NoGo
                    && c.CommandContext.ParentId == command.CommandContext.Id)))
                .Returns((Task)null);
            _eventPublisherMock.Setup(cd => cd.Publish(It.Is<ReleaseWindowUpdatedEvent>(
                c => c.ReleaseWindow == release && c.Context.ParentId == command.CommandContext.Id)))
                .Returns((Task[]) null);
            _releaseWindowGatewayMock.Setup(g => g.GetByExternalId(release.ExternalId, false, It.IsAny<bool>()))
                .Returns(existingRelease);
            _releaseWindowGatewayMock.Setup(r => r.Update(release, false));
            _releaseWindowOverlappingCheckerMock.Setup(o => o.FindOverlappedWindow(release))
                .Returns((ReleaseWindow) null);
            _releaseWindowGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(command);

            _releaseWindowGatewayMock.Verify(r => r.Update(It.IsAny<ReleaseWindow>(), It.IsAny<bool>()), Times.Once);
            _commandDispatcherMock.Verify(cd => cd.Send(It.IsAny<ClearReleaseAcknowledgesCommand>()), resetTimes);
            _commandDispatcherMock.Verify(cd => cd.Send(It.IsAny<ClearApproversSignaturesCommand>()), resetTimes);
            _commandDispatcherMock.Verify(cd => cd.Send(It.IsAny<UpdateReleaseMetricsCommand>()), resetTimes);
            _commandDispatcherMock.Verify(cd => cd.Send(It.IsAny<UpdateReleaseDecisionCommand>()), resetTimes);
            _eventPublisherMock.Verify(cd => cd.Publish(It.IsAny<ReleaseWindowUpdatedEvent>()), Times.Once);
        }

        private CommandContext ConstructContext()
        {
            return new CommandContext
            {
                UserId = Guid.NewGuid(),
                Id = Guid.NewGuid(),
            };
        }
    }
}
