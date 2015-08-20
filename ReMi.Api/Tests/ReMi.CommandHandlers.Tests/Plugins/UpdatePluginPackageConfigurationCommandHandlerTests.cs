using Autofac;
using Moq;
using NUnit.Framework;
using ReMi.CommandHandlers.Plugins;
using ReMi.Commands.Plugins;
using ReMi.Contracts.Plugins.Services;
using System;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandHandlers.Tests.Plugins
{
    [TestFixture]
    public class UpdatePluginPackageConfigurationCommandHandlerTests : TestClassFor<UpdatePluginPackageConfigurationCommandHandler>
    {
        private Mock<IContainer> _containerMock;

        protected override UpdatePluginPackageConfigurationCommandHandler ConstructSystemUnderTest()
        {
            return new UpdatePluginPackageConfigurationCommandHandler
            {
                Container = _containerMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _containerMock = new Mock<IContainer>(MockBehavior.Strict);

            _containerMock.SetupResetContainer();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallAssignGlobalPlugin_WhenCalled()
        {
            var command = new UpdatePluginPackageConfigurationCommand
            {
                PluginId = Guid.NewGuid(),
                PackageId = Guid.NewGuid(),
                JsonValues = RandomData.RandomString(10)
            };
            var pluginPackageConfigurationMock = new Mock<IPluginPackageConfiguration>(MockBehavior.Strict);

            pluginPackageConfigurationMock.Setup(x => x.SetPluginPackageConfigurationEntity(command.PackageId, command.JsonValues));
            _containerMock.SetupResolveNamed(command.PluginId.ToString().ToUpper(), pluginPackageConfigurationMock.Object);

            Sut.Handle(command);

            pluginPackageConfigurationMock.Verify(x => x.SetPluginPackageConfigurationEntity(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
        }
    }
}
