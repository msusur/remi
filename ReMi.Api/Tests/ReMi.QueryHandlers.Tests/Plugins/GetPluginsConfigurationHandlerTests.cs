using System.Collections.Generic;
using System.Linq;
using Autofac;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Data;
using ReMi.Contracts.Plugins.Services;
using ReMi.Queries.Plugins;
using ReMi.QueryHandlers.Plugins;
using System;
using ReMi.Common.Utils.Enums;

namespace ReMi.QueryHandlers.Tests.Plugins
{

    [TestFixture]
    public class GetPluginsConfigurationHandlerTests : TestClassFor<GetPluginsConfigurationHandler>
    {
        private Mock<IContainer> _containerMock;
        private Mock<Plugin.Common.PluginsConfiguration.IPluginConfiguration> _pluginConfigurationMock;

        protected override GetPluginsConfigurationHandler ConstructSystemUnderTest()
        {
            return new GetPluginsConfigurationHandler
            {
                Container = _containerMock.Object,
                PluginConfiguration = _pluginConfigurationMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _containerMock = new Mock<IContainer>(MockBehavior.Strict);
            _pluginConfigurationMock = new Mock<Plugin.Common.PluginsConfiguration.IPluginConfiguration>(MockBehavior.Strict);

            _containerMock.SetupResetContainer();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldGetGlobalPluginConfigurationAndGlobalPlugins_WhenCalled()
        {
            var pluginId = Guid.NewGuid();
            var pluginInitializerMock = new Mock<IPluginInitializer>(MockBehavior.Strict);
            pluginInitializerMock.SetupGet(i => i.Id).Returns(pluginId);
            pluginInitializerMock.SetupGet(i => i.Key).Returns(RandomData.RandomString(10));
            pluginInitializerMock.SetupGet(i => i.PluginType).Returns(RandomData.RandomEnum<PluginType>());
            pluginInitializerMock.SetupGet(i => i.IsGlobalConfigurationReadonly).Returns(true);
            pluginInitializerMock.SetupGet(i => i.IsPackageConfigurationReadonly).Returns(true);

            _pluginConfigurationMock.Setup(x => x.GetPluginInitializer(pluginId))
                .Returns(pluginInitializerMock.Object);

            var pluginConfigurationMocks = new Mock<IPluginConfiguration>(MockBehavior.Strict);
            var pluginConfigurationEntityMock = new Mock<IPluginConfigurationEntity>();
            pluginConfigurationMocks.Setup(x => x.GetPluginConfiguration())
                .Returns(pluginConfigurationEntityMock.Object);
            pluginConfigurationMocks.Setup(x => x.GetConfigurationTemplate())
                .Returns((IEnumerable<PluginConfigurationTemplate>)null);
            var pluginPacakgeConfigurationMocks = new Mock<IPluginPackageConfiguration>(MockBehavior.Strict);
            var pluginPacakgeConfigurationEntityMock = new Mock<IPluginPackageConfigurationEntity>();
            pluginPacakgeConfigurationMocks.Setup(x => x.GetPluginPackageConfiguration())
                .Returns(new[] { pluginPacakgeConfigurationEntityMock.Object });
            pluginPacakgeConfigurationMocks.Setup(x => x.GetConfigurationTemplate())
                .Returns((IEnumerable<PluginConfigurationTemplate>)null);
            _containerMock.SetupResolveNamed(pluginId.ToString().ToUpper(), pluginConfigurationMocks.Object);
            _containerMock.SetupIsRegisteredWithName(pluginId.ToString().ToUpper(), typeof(IPluginPackageConfiguration), true);
            _containerMock.SetupResolveNamed(pluginId.ToString().ToUpper(), pluginPacakgeConfigurationMocks.Object);

            var result = Sut.Handle(new GetPluginsConfigurationRequest { PluginId = pluginId });

            Assert.AreEqual(pluginId, result.Plugin.PluginId);
            Assert.AreEqual(pluginInitializerMock.Object.Key, result.Plugin.PluginKey);
            CollectionAssert.AreEqual(pluginInitializerMock.Object.PluginType.ToFlagList(), result.Plugin.PluginTypes);
            Assert.AreEqual(true, result.Plugin.IsGlobalConfigurationReadonly);
            Assert.AreEqual(true, result.Plugin.IsPackageConfigurationReadonly);

            pluginConfigurationMocks.Verify(x => x.GetPluginConfiguration(), Times.Once);
            pluginConfigurationMocks.Verify(x => x.GetConfigurationTemplate(), Times.Once);
            pluginPacakgeConfigurationMocks.Verify(x => x.GetPluginPackageConfiguration(), Times.Once);
            pluginPacakgeConfigurationMocks.Verify(x => x.GetConfigurationTemplate(), Times.Once);
        }

        [Test]
        public void Handle_ShouldNotThrowException_WhenPackageConfigurationIsEmpty()
        {
            var pluginId = Guid.NewGuid();
            var pluginInitializerMock = new Mock<IPluginInitializer>(MockBehavior.Strict);
            pluginInitializerMock.SetupGet(i => i.Id).Returns(pluginId);
            pluginInitializerMock.SetupGet(i => i.Key).Returns(RandomData.RandomString(10));
            pluginInitializerMock.SetupGet(i => i.PluginType).Returns(RandomData.RandomEnum<PluginType>());
            pluginInitializerMock.SetupGet(i => i.IsGlobalConfigurationReadonly).Returns(true);
            pluginInitializerMock.SetupGet(i => i.IsPackageConfigurationReadonly).Returns(true);

            _pluginConfigurationMock.Setup(x => x.GetPluginInitializer(pluginId))
                .Returns(pluginInitializerMock.Object);

            var pluginConfigurationMocks = new Mock<IPluginConfiguration>(MockBehavior.Strict);
            var pluginConfigurationEntityMock = new Mock<IPluginConfigurationEntity>();
            pluginConfigurationMocks.Setup(x => x.GetPluginConfiguration())
                .Returns(pluginConfigurationEntityMock.Object);
            pluginConfigurationMocks.Setup(x => x.GetConfigurationTemplate())
                .Returns((IEnumerable<PluginConfigurationTemplate>)null);
            var pluginPacakgeConfigurationMocks = new Mock<IPluginPackageConfiguration>(MockBehavior.Strict);
            pluginPacakgeConfigurationMocks.Setup(x => x.GetPluginPackageConfiguration())
                .Returns((IEnumerable<IPluginPackageConfigurationEntity>)null);
            pluginPacakgeConfigurationMocks.Setup(x => x.GetConfigurationTemplate())
                .Returns((IEnumerable<PluginConfigurationTemplate>)null);
            _containerMock.SetupResolveNamed(pluginInitializerMock.Object.Id.ToString().ToUpper(), pluginConfigurationMocks.Object);
            _containerMock.SetupIsRegisteredWithName(pluginInitializerMock.Object.Id.ToString().ToUpper(), typeof(IPluginPackageConfiguration), true);
            _containerMock.SetupResolveNamed(pluginInitializerMock.Object.Id.ToString().ToUpper(), pluginPacakgeConfigurationMocks.Object);

            Sut.Handle(new GetPluginsConfigurationRequest { PluginId = pluginId });

            pluginConfigurationMocks.Verify(x => x.GetPluginConfiguration(), Times.Once);
            pluginConfigurationMocks.Verify(x => x.GetConfigurationTemplate(), Times.Once);
            pluginPacakgeConfigurationMocks.Verify(x => x.GetPluginPackageConfiguration(), Times.Once);
            pluginPacakgeConfigurationMocks.Verify(x => x.GetConfigurationTemplate(), Times.Once);
        }

        [Test]
        public void Handle_ShouldReturnNullInPackageConfiguration_WhenPackageConfigurationIsNotRegistered()
        {
            var pluginId = Guid.NewGuid();
            var pluginInitializerMock = new Mock<IPluginInitializer>(MockBehavior.Strict);
            pluginInitializerMock.SetupGet(i => i.Id).Returns(pluginId);
            pluginInitializerMock.SetupGet(i => i.Key).Returns(RandomData.RandomString(10));
            pluginInitializerMock.SetupGet(i => i.PluginType).Returns(RandomData.RandomEnum<PluginType>());
            pluginInitializerMock.SetupGet(i => i.IsGlobalConfigurationReadonly).Returns(true);
            pluginInitializerMock.SetupGet(i => i.IsPackageConfigurationReadonly).Returns(true);

            _pluginConfigurationMock.Setup(x => x.GetPluginInitializer(pluginId))
                .Returns(pluginInitializerMock.Object);

            var pluginConfigurationMocks = new Mock<IPluginConfiguration>(MockBehavior.Strict);
            var pluginConfigurationEntityMock = new Mock<IPluginConfigurationEntity>();
            pluginConfigurationMocks.Setup(x => x.GetPluginConfiguration())
                .Returns(pluginConfigurationEntityMock.Object);
            pluginConfigurationMocks.Setup(x => x.GetConfigurationTemplate())
                .Returns((IEnumerable<PluginConfigurationTemplate>)null);
            _containerMock.SetupResolveNamed(pluginInitializerMock.Object.Id.ToString().ToUpper(), pluginConfigurationMocks.Object);
            _containerMock.SetupIsRegisteredWithName(pluginInitializerMock.Object.Id.ToString().ToUpper(), typeof(IPluginPackageConfiguration), false);

            var result = Sut.Handle(new GetPluginsConfigurationRequest { PluginId = pluginId });


            Assert.IsNull(result.Plugin.PackageConfiguration);
            Assert.IsNull(result.Plugin.PackageConfigurationTemplates);
            pluginConfigurationMocks.Verify(x => x.GetPluginConfiguration(), Times.Once);
            pluginConfigurationMocks.Verify(x => x.GetConfigurationTemplate(), Times.Once);
        }
    }
}
