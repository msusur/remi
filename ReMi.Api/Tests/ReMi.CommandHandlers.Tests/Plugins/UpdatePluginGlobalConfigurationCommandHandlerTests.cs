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
    public class UpdatePluginGlobalConfigurationCommandHandlerTests : TestClassFor<UpdatePluginGlobalConfigurationCommandHandler>
    {
        private Mock<IContainer> _containerMock;

        protected override UpdatePluginGlobalConfigurationCommandHandler ConstructSystemUnderTest()
        {
            return new UpdatePluginGlobalConfigurationCommandHandler
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
            var command = new UpdatePluginGlobalConfigurationCommand
            {
                PluginId = Guid.NewGuid(),
                JsonValues = RandomData.RandomString(10)
            };
            var pluginConfigurationMock = new Mock<IPluginConfiguration>(MockBehavior.Strict);

            pluginConfigurationMock.Setup(x => x.SetPluginConfiguration(command.JsonValues));
            _containerMock.SetupResolveNamed(command.PluginId.ToString().ToUpper(), pluginConfigurationMock.Object);

            Sut.Handle(command);

            pluginConfigurationMock.Verify(x => x.SetPluginConfiguration(It.IsAny<string>()), Times.Once);
        }
    }
}
