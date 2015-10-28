using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.DeploymentTool;
using ReMi.Commands.Metrics;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.EventHandlers.Metrics;
using ReMi.Events.Metrics;
using ReMi.TestUtils.UnitTests;

namespace ReMi.EventHandlers.Tests.Metrics
{
    public class DeploymentMeasurementsPopulatedEventHandlerTests : TestClassFor<DeploymentMeasurementsPopulatedEventHandler>
    {
        private Mock<ICommandDispatcher> _commandDispatcher;

        protected override DeploymentMeasurementsPopulatedEventHandler ConstructSystemUnderTest()
        {
            return new DeploymentMeasurementsPopulatedEventHandler
            {
                CommandDispatcher = _commandDispatcher.Object
            };
        }

        protected override void TestInitialize()
        {
            _commandDispatcher = new Mock<ICommandDispatcher>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldSendCommad_WhenHandled()
        {
            var evt = new DeploymentMeasurementsPopulatedEvent
            {
                ReleaseWindowId = Guid.NewGuid(),
                Context = new EventContext { Id = Guid.NewGuid() }
            };

            _commandDispatcher.Setup(x => x.Send(It.Is<CalculateDeployTimeCommand>(c =>
                c.ReleaseWindowId == evt.ReleaseWindowId
                && c.CommandContext.ParentId == evt.Context.Id)))
                .Returns((Task) null);
            
            Sut.Handle(evt);

            _commandDispatcher.Verify(x => x.Send(It.IsAny<ICommand>()), Times.Once);
        }
    }
}
