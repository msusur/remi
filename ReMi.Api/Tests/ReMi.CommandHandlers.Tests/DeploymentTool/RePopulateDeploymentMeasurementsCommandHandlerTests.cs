using Moq;
using NUnit.Framework;
using ReMi.CommandHandlers.DeploymentTool;
using ReMi.Commands.DeploymentTool;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseExecution;
using System;
using System.Threading.Tasks;

namespace ReMi.CommandHandlers.Tests.DeploymentTool
{
    [TestFixture]
    public class RePopulateDeploymentMeasurementsCommandHandlerTests : TestClassFor<RePopulateDeploymentMeasurementsCommandHandler>
    {
        private Mock<IReleaseDeploymentMeasurementGateway> _releaseDeploymentMeasurementGatewayMock;
        private Mock<ICommandDispatcher> _commandDispatcherMock;


        protected override RePopulateDeploymentMeasurementsCommandHandler ConstructSystemUnderTest()
        {
            return new RePopulateDeploymentMeasurementsCommandHandler
            {
                ReleaseDeploymentMeasurementGatewayFactory = () => _releaseDeploymentMeasurementGatewayMock.Object,
                CommandDispatcher = _commandDispatcherMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _commandDispatcherMock = new Mock<ICommandDispatcher>(MockBehavior.Strict);
            _releaseDeploymentMeasurementGatewayMock = new Mock<IReleaseDeploymentMeasurementGateway>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldRemoveMeasurementsFromDbAndSendCommandToPopulateAgain_WhenCalled()
        {
            var command = new RePopulateDeploymentMeasurementsCommand
            {
                ReleaseWindowId = Guid.NewGuid(),
                CommandContext = new CommandContext {Id = Guid.NewGuid()}
            };
            _releaseDeploymentMeasurementGatewayMock.Setup(x => x.RemoveDeploymentMeasurements(command.ReleaseWindowId));
            _commandDispatcherMock.Setup(x => x.Send(It.Is<PopulateDeploymentMeasurementsCommand>(c =>
                c.ReleaseWindowId == command.ReleaseWindowId
                && c.CommandContext.ParentId == command.CommandContext.Id)))
                .Returns((Task) null);
            _releaseDeploymentMeasurementGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(command);

            _releaseDeploymentMeasurementGatewayMock.Verify(x => x.RemoveDeploymentMeasurements(It.IsAny<Guid>()), Times.Once);
            _commandDispatcherMock.Verify(x => x.Send(It.IsAny<ICommand>()), Times.Once);
        }
    }
}
