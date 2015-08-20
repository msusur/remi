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
    public class GetPackagePluginConfigurationEntityHandlerTests : TestClassFor<GetPackagePluginConfigurationEntityHandler>
    {
        private Mock<IContainer> _containerMock;

        protected override GetPackagePluginConfigurationEntityHandler ConstructSystemUnderTest()
        {
            return new GetPackagePluginConfigurationEntityHandler
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
            var request = new GetPackagePluginConfigurationEntityRequest
            {
                PluginId = Guid.NewGuid(),
                PackageId = Guid.NewGuid()
            };
            var pluginConfigurationMock = new Mock<IPluginPackageConfiguration>(MockBehavior.Strict);
            var pluginConfigurationEntityMock = new Mock<IPluginPackageConfigurationEntity>();

            pluginConfigurationMock.Setup(x => x.GetPluginPackageConfigurationEntity(request.PackageId))
                .Returns(pluginConfigurationEntityMock.Object);
            _containerMock.SetupResolveNamed(request.PluginId.ToString().ToUpper(), pluginConfigurationMock.Object);

            var result = Sut.Handle(request);

            Assert.AreEqual(pluginConfigurationEntityMock.Object, result.PackageConfiguration);
            pluginConfigurationMock.Verify(x => x.GetPluginPackageConfigurationEntity(It.IsAny<Guid>()), Times.Once);
        }
    }
}
