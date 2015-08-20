using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.DeploymentTool;
using ReMi.CommandHandlers.ReleasePlan;
using ReMi.Commands.ReleasePlan;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using System;

namespace ReMi.CommandHandlers.Tests.ReleasePlan
{
    public class UpdateReleaseJobCommandHandlerTests : TestClassFor<UpdateReleaseJobCommandHandler>
    {
        private Mock<IReleaseJobGateway> _releaseJobGatewayFactoryMock;

        protected override UpdateReleaseJobCommandHandler ConstructSystemUnderTest()
        {
            return new UpdateReleaseJobCommandHandler
            {
                ReleaseJobFuncGatewayFactory = () => _releaseJobGatewayFactoryMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseJobGatewayFactoryMock = new Mock<IReleaseJobGateway>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_RemoveJobFromRelease_WhenJobExistsAndNotIncluded()
        {
            var command = new UpdateReleaseJobCommand
            {
                ReleaseJob = new ReleaseJob { JobId = Guid.NewGuid(), IsIncluded = false },
                ReleaseWindowId = Guid.NewGuid()
            };
            _releaseJobGatewayFactoryMock.Setup(x => x.GetReleaseJob(command.ReleaseWindowId, command.ReleaseJob.JobId))
                .Returns(command.ReleaseJob);
            _releaseJobGatewayFactoryMock.Setup(
                x => x.RemoveJobFromRelease(command.ReleaseJob.JobId, command.ReleaseWindowId));
            _releaseJobGatewayFactoryMock.Setup(x => x.Dispose());

            Sut.Handle(command);

            _releaseJobGatewayFactoryMock.Verify(x => x.GetReleaseJob(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
            _releaseJobGatewayFactoryMock.Verify(x => x.RemoveJobFromRelease(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
            _releaseJobGatewayFactoryMock.Verify(x => x.AddJobToRelease(It.IsAny<ReleaseJob>(), It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void Handle_AddJobToRelease_WhenJobNotExistsAndIncluded()
        {
            var command = new UpdateReleaseJobCommand
            {
                ReleaseJob = new ReleaseJob { JobId = Guid.NewGuid(), IsIncluded = true },
                ReleaseWindowId = Guid.NewGuid()
            };
            _releaseJobGatewayFactoryMock.Setup(x => x.GetReleaseJob(command.ReleaseWindowId, command.ReleaseJob.JobId))
                .Returns((ReleaseJob) null);
            _releaseJobGatewayFactoryMock.Setup(
                x => x.AddJobToRelease(command.ReleaseJob, command.ReleaseWindowId));
            _releaseJobGatewayFactoryMock.Setup(x => x.Dispose());

            Sut.Handle(command);

            _releaseJobGatewayFactoryMock.Verify(x => x.GetReleaseJob(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
            _releaseJobGatewayFactoryMock.Verify(x => x.RemoveJobFromRelease(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            _releaseJobGatewayFactoryMock.Verify(x => x.AddJobToRelease(It.IsAny<ReleaseJob>(), It.IsAny<Guid>()), Times.Once);
        }

    }
}
