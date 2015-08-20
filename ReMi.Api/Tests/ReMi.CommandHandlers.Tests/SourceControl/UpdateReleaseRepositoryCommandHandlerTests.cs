using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.CommandHandlers.SourceControl;
using ReMi.Commands.SourceControl;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Data.SourceControl;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using System;

namespace ReMi.CommandHandlers.Tests.SourceControl
{
    [TestFixture]
    public class UpdateReleaseRepositoryCommandHandlerTests : TestClassFor<UpdateReleaseRepositoryCommandHandler>
    {
        private Mock<IReleaseRepositoryGateway> _releaseRepositoryGateway;

        protected override UpdateReleaseRepositoryCommandHandler ConstructSystemUnderTest()
        {
            return new UpdateReleaseRepositoryCommandHandler
            {
                ReleaseRepositoryGatewayFactory = () => _releaseRepositoryGateway.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseRepositoryGateway = new Mock<IReleaseRepositoryGateway>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldAddRepository_WhenNotExists()
        {
            var command = new UpdateReleaseRepositoryCommand
            {
                ReleaseWindowId = Guid.NewGuid(),
                Repository = Builder<ReleaseRepository>.CreateNew().Build()
            };

            _releaseRepositoryGateway.Setup(x => x.GetReleaseRepository(command.ReleaseWindowId, command.Repository.ExternalId))
                .Returns((ReleaseRepository)null);
            _releaseRepositoryGateway.Setup(x => x.AddRepositoryToRelease(command.Repository, command.ReleaseWindowId));
            _releaseRepositoryGateway.Setup(x => x.Dispose());

            Sut.Handle(command);

            _releaseRepositoryGateway.Verify(x => x.UpdateRepository(It.IsAny<ReleaseRepository>(), It.IsAny<Guid>()), Times.Never);
            _releaseRepositoryGateway.Verify(x => x.AddRepositoryToRelease(It.IsAny<ReleaseRepository>(), It.IsAny<Guid>()), Times.Once);
        }
        [Test]
        public void Handle_ShouldUpdateRepository_WhenExists()
        {
            var command = new UpdateReleaseRepositoryCommand
            {
                ReleaseWindowId = Guid.NewGuid(),
                Repository = Builder<ReleaseRepository>.CreateNew().Build()
            };

            _releaseRepositoryGateway.Setup(x => x.GetReleaseRepository(command.ReleaseWindowId, command.Repository.ExternalId))
                .Returns(command.Repository);
            _releaseRepositoryGateway.Setup(x => x.UpdateRepository(command.Repository, command.ReleaseWindowId));
            _releaseRepositoryGateway.Setup(x => x.Dispose());

            Sut.Handle(command);

            _releaseRepositoryGateway.Verify(x => x.UpdateRepository(It.IsAny<ReleaseRepository>(), It.IsAny<Guid>()), Times.Once);
            _releaseRepositoryGateway.Verify(x => x.AddRepositoryToRelease(It.IsAny<ReleaseRepository>(), It.IsAny<Guid>()), Times.Never);
        }
    }
}
