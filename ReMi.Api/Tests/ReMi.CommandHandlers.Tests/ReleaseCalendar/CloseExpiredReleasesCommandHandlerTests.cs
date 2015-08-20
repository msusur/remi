using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessEntities.ReleaseExecution;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.BusinessLogic.ReleasePlan;
using ReMi.CommandHandlers.ReleaseCalendar;
using ReMi.Commands.ReleaseCalendar;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseExecution;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;

namespace ReMi.CommandHandlers.Tests.ReleaseCalendar
{
    public class CloseExpiredReleasesCommandHandlerTests
        : TestClassFor<CloseExpiredReleasesCommandHandler>
    {
        private Mock<IReleaseWindowGateway> _releaseWindowGatewayMock;
        private Mock<IReleaseWindowStateUpdater> _releaseWindowStateUpdaterMock;
        private Mock<IReleaseApproverGateway> _approversGatewayMock;
        private Mock<ISignOffGateway> _signersGatewayMock;
        private Mock<IReleaseWindowHelper> _releaseWindowHelperMock;

        protected override CloseExpiredReleasesCommandHandler ConstructSystemUnderTest()
        {
            return new CloseExpiredReleasesCommandHandler
            {
                ReleaseWindowGatewayFactory = () => _releaseWindowGatewayMock.Object,
                SignersGatewayFactory = () => _signersGatewayMock.Object,
                ApproversGatewayFactory = () => _approversGatewayMock.Object,
                ReleaseWindowStateUpdater = _releaseWindowStateUpdaterMock.Object,
                ReleaseWindowHelper = _releaseWindowHelperMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseWindowGatewayMock = new Mock<IReleaseWindowGateway>();
            _releaseWindowStateUpdaterMock = new Mock<IReleaseWindowStateUpdater>();
            _approversGatewayMock = new Mock<IReleaseApproverGateway>();
            _signersGatewayMock = new Mock<ISignOffGateway>();
            _releaseWindowHelperMock = new Mock<IReleaseWindowHelper>();
            _releaseWindowHelperMock.Setup(x => x.IsMaintenance(It.IsAny<ReleaseWindow>())).Returns(true);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallStateUpdaterToCloseRelease_WhenReleaseWasSigned()
        {
            var releaseWindow = new ReleaseWindow
            {
                ExternalId = Guid.NewGuid(),
                ApprovedOn = DateTime.UtcNow,
                SignedOff = DateTime.UtcNow,
                ReleaseType = ReleaseType.SystemMaintenance
            };

            _releaseWindowGatewayMock.Setup(x => x.GetExpiredReleases()).Returns(new[] { releaseWindow });

            Sut.Handle(new CloseExpiredReleasesCommand
            {
                CommandContext = new CommandContext { UserId = Guid.NewGuid() }
            });

            _releaseWindowStateUpdaterMock.Verify(
                x =>
                    x.CloseRelease(releaseWindow.ExternalId, "Closing automatically due to expiration",
                        It.IsAny<IEnumerable<Account>>(), It.IsAny<Guid>()));
        }

        [Test]
        public void Handle_ShouldCallStateUpdaterToCloseRelease_WhenReleaseDoesntHaveAnySignersOrApprovers()
        {
            var releaseWindow = new ReleaseWindow
            {
                ExternalId = Guid.NewGuid(),
                ReleaseType = ReleaseType.SystemMaintenance
            };

            _releaseWindowGatewayMock.Setup(x => x.GetExpiredReleases()).Returns(new[] { releaseWindow });
            _approversGatewayMock.Setup(x => x.GetApprovers(releaseWindow.ExternalId))
                .Returns(new List<ReleaseApprover>());
            _signersGatewayMock.Setup(x => x.GetSignOffs(releaseWindow.ExternalId))
                .Returns(new List<SignOff>());

            Sut.Handle(new CloseExpiredReleasesCommand
            {
                CommandContext = new CommandContext { UserId = Guid.NewGuid() }
            });

            _releaseWindowStateUpdaterMock.Verify(
                x =>
                    x.CloseRelease(releaseWindow.ExternalId, "Closing automatically due to expiration",
                        It.IsAny<IEnumerable<Account>>(), It.IsAny<Guid>()));
            _approversGatewayMock.Verify(x => x.GetApprovers(releaseWindow.ExternalId));
            _signersGatewayMock.Verify(x => x.GetSignOffs(releaseWindow.ExternalId));
        }

        [Test]
        public void Handle_ShouldNotCallStateUpdaterToCloseRelease_WhenReleaseWasApprovedAndItHasSigners()
        {
            var releaseWindow = new ReleaseWindow
            {
                ExternalId = Guid.NewGuid(),
                ApprovedOn = DateTime.UtcNow,
                ReleaseType = ReleaseType.SystemMaintenance
            };

            _releaseWindowGatewayMock.Setup(x => x.GetExpiredReleases()).Returns(new[] { releaseWindow });
            _signersGatewayMock.Setup(x => x.GetSignOffs(releaseWindow.ExternalId))
                .Returns(new List<SignOff> { new SignOff() });

            Sut.Handle(new CloseExpiredReleasesCommand
            {
                CommandContext = new CommandContext { UserId = Guid.NewGuid() }
            });

            _releaseWindowStateUpdaterMock.Verify(
                x =>
                    x.CloseRelease(releaseWindow.ExternalId, "Closing automatically due to expiration",
                        It.IsAny<IEnumerable<Account>>(), It.IsAny<Guid>()), Times.Never);
            _signersGatewayMock.Verify(x => x.GetSignOffs(releaseWindow.ExternalId));
        }

        [Test]
        public void Handle_ShouldNotCallStateUpdaterToCloseRelease_WhenReleaseWasNotApprovedAndItHasApprovers()
        {
            var releaseWindow = new ReleaseWindow
            {
                ExternalId = Guid.NewGuid(),
                ReleaseType = ReleaseType.SystemMaintenance
            };

            _releaseWindowGatewayMock.Setup(x => x.GetExpiredReleases()).Returns(new[] { releaseWindow });
            _approversGatewayMock.Setup(x => x.GetApprovers(releaseWindow.ExternalId))
                .Returns(new List<ReleaseApprover> { new ReleaseApprover() });

            Sut.Handle(new CloseExpiredReleasesCommand
            {
                CommandContext = new CommandContext { UserId = Guid.NewGuid() }
            });

            _releaseWindowStateUpdaterMock.Verify(
                x =>
                    x.CloseRelease(releaseWindow.ExternalId, "Closing automatically due to expiration",
                        It.IsAny<IEnumerable<Account>>(), It.IsAny<Guid>()), Times.Never);
            _approversGatewayMock.Verify(x => x.GetApprovers(releaseWindow.ExternalId));
        }


        [Test]
        public void Handle_ShouldNotCallStateUpdaterToCloseRelease_WhenReleaseIsNotMaintenance()
        {
            var releaseWindow = new ReleaseWindow
            {
                ExternalId = Guid.NewGuid(),
                ApprovedOn = DateTime.UtcNow,
                SignedOff = DateTime.UtcNow,
                ReleaseType = ReleaseType.Scheduled
            };
            _releaseWindowHelperMock.Setup(x => x.IsMaintenance(It.IsAny<ReleaseWindow>())).Returns(false);

            _releaseWindowGatewayMock.Setup(x => x.GetExpiredReleases()).Returns(new[] { releaseWindow });

            Sut.Handle(new CloseExpiredReleasesCommand
            {
                CommandContext = new CommandContext { UserId = Guid.NewGuid() }
            });

            _approversGatewayMock.Verify(x => x.GetApprovers(It.IsAny<Guid>()), Times.Never);
            _signersGatewayMock.Verify(x => x.GetSignOffs(It.IsAny<Guid>()), Times.Never);
            _releaseWindowStateUpdaterMock.Verify(
                x =>
                    x.CloseRelease(It.IsAny<Guid>(), It.IsAny<string>(),
                        It.IsAny<IEnumerable<Account>>(), It.IsAny<Guid>()), Times.Never);
        }
    }
}
