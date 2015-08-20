using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Data;
using ReMi.Contracts.Plugins.Services;
using ReMi.Queries.Plugins;
using ReMi.QueryHandlers.Plugins;
using System;
using System.Linq;
using ReMi.Common.Utils.Enums;

namespace ReMi.QueryHandlers.Tests.Plugins
{

    [TestFixture]
    public class GetPluginsHandlerTests : TestClassFor<GetPluginsHandler>
    {
        private Mock<Plugin.Common.PluginsConfiguration.IPluginConfiguration> _pluginConfigurationMock;

        protected override GetPluginsHandler ConstructSystemUnderTest()
        {
            return new GetPluginsHandler
            {
                PluginConfiguration = _pluginConfigurationMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _pluginConfigurationMock = new Mock<Plugin.Common.PluginsConfiguration.IPluginConfiguration>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldReturnsAllPlugins_WhenCalled()
        {
            const int pluginCount = 5;

            var pluginInitializesMocks = Builder<Mock<IPluginInitializer>>.CreateListOfSize(pluginCount)
                .All()
                .WithConstructor(() => new Mock<IPluginInitializer>(MockBehavior.Strict))
                .Do(x => x.SetupGet(i => i.Id).Returns(Guid.NewGuid()))
                .Do(x => x.SetupGet(i => i.Key).Returns(Guid.NewGuid().ToString))
                .Do(x => x.SetupGet(i => i.PluginType).Returns(RandomData.RandomEnum<PluginType>()))
                .Do(x => x.SetupGet(i => i.IsPackageConfigurationReadonly).Returns(RandomData.RandomBool()))
                .Do(x => x.SetupGet(i => i.IsGlobalConfigurationReadonly).Returns(RandomData.RandomBool()))
                .Build().ToList();
            _pluginConfigurationMock.SetupGet(x => x.PluginInitializers)
                .Returns(pluginInitializesMocks.Select(x => x.Object).ToList());


            var response = Sut.Handle(new GetPluginsRequest());

            Assert.AreEqual(pluginCount, response.Plugins.Count());
            CollectionAssert.AreEquivalent(pluginInitializesMocks.Select(x => x.Object.Id), response.Plugins.Select(x => x.PluginId));
            CollectionAssert.AreEquivalent(pluginInitializesMocks.Select(x => x.Object.Key), response.Plugins.Select(x => x.PluginKey));
            CollectionAssert.AreEquivalent(pluginInitializesMocks.SelectMany(x => x.Object.PluginType.ToFlagList()), response.Plugins.SelectMany(x => x.PluginTypes));
            CollectionAssert.AreEquivalent(pluginInitializesMocks.Select(x => x.Object.IsGlobalConfigurationReadonly), response.Plugins.Select(x => x.IsGlobalConfigurationReadonly));
            CollectionAssert.AreEquivalent(pluginInitializesMocks.Select(x => x.Object.IsPackageConfigurationReadonly), response.Plugins.Select(x => x.IsPackageConfigurationReadonly));

            _pluginConfigurationMock.VerifyGet(x => x.PluginInitializers, Times.Once);
        }
    }
}
