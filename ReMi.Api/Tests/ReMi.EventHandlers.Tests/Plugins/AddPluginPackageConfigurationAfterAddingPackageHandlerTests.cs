using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Products;
using ReMi.Commands.Plugins;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.EventHandlers.Plugins;
using ReMi.Events.Packages;
using System;
using System.Threading.Tasks;
using ReMi.TestUtils.UnitTests;

namespace ReMi.EventHandlers.Tests.Plugins
{
    [TestFixture]
    public class AddPluginPackageConfigurationAfterAddingPackageHandlerTests : TestClassFor<AddPluginPackageConfigurationAfterAddingPackageHandler>
    {
        private Mock<ICommandDispatcher> _commandDispatcherMock;

        protected override AddPluginPackageConfigurationAfterAddingPackageHandler ConstructSystemUnderTest()
        {
            return new AddPluginPackageConfigurationAfterAddingPackageHandler
            {
                CommandDispatcher = _commandDispatcherMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _commandDispatcherMock = new Mock<ICommandDispatcher>(MockBehavior.Strict);
            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldSendCommandToAddNewConfiguration_WhenPackageIsNotNull()
        {
            var evt = new NewPackageAddedEvent
            {
                Package = new Product { ExternalId = Guid.NewGuid() }
            };

            _commandDispatcherMock.Setup(x => x.Send(It.Is<AddPackagePluginConfigurationCommand>(
                c => c.PackageId == evt.Package.ExternalId)))
                .Returns((Task)null);

            Sut.Handle(evt);

            _commandDispatcherMock.Verify(x => x.Send(It.IsAny<AddPackagePluginConfigurationCommand>()), Times.Once);
        }

        [Test]
        public void Handle_ShouldDoNothin_WhenPackageIsEmpty()
        {
            var evt = new NewPackageAddedEvent
            {
                Package = null
            };

            Sut.Handle(evt);

            _commandDispatcherMock.Verify(x => x.Send(It.IsAny<AddPackagePluginConfigurationCommand>()), Times.Never);
        }
    }
}
