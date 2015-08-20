using Autofac;
using Moq;
using NUnit.Framework;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Data;
using ReMi.Contracts.Plugins.Services;
using ReMi.Queries.Plugins;
using ReMi.QueryHandlers.Plugins;
using System;

namespace ReMi.QueryHandlers.Tests.Plugins
{

    [TestFixture]
    public class GetGlobalPluginConfigurationEntityHandlerTests : TestClassFor<GetGlobalPluginConfigurationEntityHandler>
    {
        private Mock<IContainer> _containerMock;

        protected override GetGlobalPluginConfigurationEntityHandler ConstructSystemUnderTest()
        {
            return new GetGlobalPluginConfigurationEntityHandler
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
        public void Handle_ShouldGetGlobalPluginConfigurationAndGlobalPlugins_WhenCalled()
        {
            var request = new GetGlobalPluginConfigurationEntityRequest
            {
                PluginId = Guid.NewGuid()
            };
            var pluginConfigurationMock = new Mock<IPluginConfiguration>(MockBehavior.Strict);
            var pluginConfigurationEntityMock = new Mock<IPluginConfigurationEntity>();

            pluginConfigurationMock.Setup(x => x.GetPluginConfiguration())
                .Returns(pluginConfigurationEntityMock.Object);
            _containerMock.SetupResolveNamed(request.PluginId.ToString().ToUpper(), pluginConfigurationMock.Object);

            var result = Sut.Handle(request);

            Assert.AreEqual(pluginConfigurationEntityMock.Object, result.GlobalConfiguration);
            pluginConfigurationMock.Verify(x => x.GetPluginConfiguration(), Times.Once);
        }
    }
}
