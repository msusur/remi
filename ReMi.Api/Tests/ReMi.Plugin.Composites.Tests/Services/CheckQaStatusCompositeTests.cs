using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Plugins;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Data;
using ReMi.Contracts.Plugins.Data.QaStats;
using ReMi.Contracts.Plugins.Services.QaStats;
using ReMi.DataAccess.BusinessEntityGateways.Plugins;
using ReMi.Plugin.Composites.Services;

namespace ReMi.Plugin.Composites.Tests.Services
{
    public class CheckQaStatusCompositeTests : TestClassFor<CheckQaStatusComposite>
    {
        private Mock<IPluginGateway> _pluginGatewayMock;
        private Mock<IContainer> _containerMock;

        protected override CheckQaStatusComposite ConstructSystemUnderTest()
        {
            return new CheckQaStatusComposite
            {
                Container = _containerMock.Object,
                PluginGatewayFactory = () => _pluginGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _containerMock = new Mock<IContainer>(MockBehavior.Strict);
            _pluginGatewayMock = new Mock<IPluginGateway>(MockBehavior.Strict);

            _containerMock.SetupResetContainer();

            base.TestInitialize();
        }

        [Test]
        public void GetQaStatusCheckItems_ShouldGetQaStatsFromProperPlugin_WhenPluginIsAssignToThePackage()
        {
            var packagePluginConfiguration = Builder<PackagePluginConfiguration>.CreateNew()
                .With(x => x.PluginId, Guid.NewGuid())
                .With(x => x.PackageId, Guid.NewGuid())
                .Build();
            var expected = new List<QaStatusCheckItem>();

            _pluginGatewayMock.Setup(x => x.GetPackagePluginConfiguration(packagePluginConfiguration.PackageId, PluginType.QaStats))
                .Returns(packagePluginConfiguration);
            _pluginGatewayMock.Setup(x => x.Dispose());

            var checkQaStatusMock = new Mock<ICheckQaStatus>(MockBehavior.Strict);
            checkQaStatusMock.Setup(x => x.GetQaStatusCheckItems(It.Is<IEnumerable<Guid>>(ids => ids.First() == packagePluginConfiguration.PackageId)))
                .Returns(expected);
            _containerMock.SetupResolveNamed(packagePluginConfiguration.PluginId.ToString().ToUpper(), checkQaStatusMock.Object);

            var actual = Sut.GetQaStatusCheckItems(new [] { packagePluginConfiguration.PackageId });

            Assert.AreEqual(expected, actual);
            _pluginGatewayMock.Verify(x => x.GetPackagePluginConfiguration(It.IsAny<Guid>(), It.IsAny<PluginType>()), Times.Once);
            checkQaStatusMock.Verify(x => x.GetQaStatusCheckItems(It.IsAny<IEnumerable<Guid>>()), Times.Once);
        }

        [Test]
        public void GetQaStatusCheckItems_ShouldReturnNull_WhenPluginIsNotAssignToThePackage()
        {
            var packagePluginConfiguration = Builder<PackagePluginConfiguration>.CreateNew()
                .With(x => x.PluginId, null)
                .With(x => x.PackageId, Guid.NewGuid())
                .Build();
            _pluginGatewayMock.Setup(x => x.GetPackagePluginConfiguration(packagePluginConfiguration.PackageId, PluginType.QaStats))
                .Returns(packagePluginConfiguration);
            _pluginGatewayMock.Setup(x => x.Dispose());

            var actual = Sut.GetQaStatusCheckItems(new[] { packagePluginConfiguration.PackageId });

            Assert.IsEmpty(actual);
            _pluginGatewayMock.Verify(x => x.GetPackagePluginConfiguration(It.IsAny<Guid>(), It.IsAny<PluginType>()), Times.Once);
        }
    }
}
