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
using ReMi.DataAccess.BusinessEntityGateways.Plugins;
using ReMi.Plugin.Composites.Services;
using ReMi.Contracts.Plugins.Data.DeploymentTool;
using ReMi.Contracts.Plugins.Services.DeploymentTool;

namespace ReMi.Plugin.Composites.Tests.Services
{
    public class DeploymentToolCompositeTests : TestClassFor<DeploymentToolComposite>
    {
        private Mock<IPluginGateway> _pluginGatewayMock;
        private Mock<IContainer> _containerMock;

        protected override DeploymentToolComposite ConstructSystemUnderTest()
        {
            return new DeploymentToolComposite
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
        public void GetReleaseJobs_ShouldReturnReleaseJobsForManyPackages_WhenListOfPackagesAreAssignedToMoreThenOnePlugin()
        {
            const int packagePluginConfigurationCount = 5;

            var job1 = Builder<ReleaseJob>.CreateListOfSize(1).Build();
            var job2 = Builder<ReleaseJob>.CreateListOfSize(1).Build();
            var job3 = Builder<ReleaseJob>.CreateListOfSize(1).Build();
            var packagePluginConfigurations = Builder<PackagePluginConfiguration>.CreateListOfSize(packagePluginConfigurationCount)
                .All()
                .Do(x => x.PluginId = Guid.NewGuid())
                .Do(x => x.PackageId = Guid.NewGuid())
                .Build();
            packagePluginConfigurations[1].PluginId = packagePluginConfigurations[4].PluginId;
            packagePluginConfigurations[3].PluginId = null;
            var group1 = new[] { packagePluginConfigurations[0] };
            var group2 = new[] { packagePluginConfigurations[1], packagePluginConfigurations[4] };
            var group3 = new[] { packagePluginConfigurations[2] };

            foreach (var packagePluginConfiguration in packagePluginConfigurations)
            {
                var configuration = packagePluginConfiguration;
                _pluginGatewayMock.Setup(x => x.GetPackagePluginConfiguration(configuration.PackageId, PluginType.DeploymentTool))
                    .Returns(packagePluginConfiguration);
            }
            _pluginGatewayMock.Setup(x => x.Dispose());

            var releaseContentMock1 = new Mock<IDeploymentTool>(MockBehavior.Strict);
            releaseContentMock1.Setup(x => x.GetReleaseJobs(It.Is<IEnumerable<Guid>>(ids => ids.All(id => group1.Any(p => p.PackageId == id)))))
                .Returns(job1);
            _containerMock.SetupResolveNamed(group1.First().PluginId.ToString().ToUpper(), releaseContentMock1.Object);
            var releaseContentMock2 = new Mock<IDeploymentTool>(MockBehavior.Strict);
            releaseContentMock2.Setup(x => x.GetReleaseJobs(It.Is<IEnumerable<Guid>>(ids => ids.All(id => group2.Any(p => p.PackageId == id)))))
                .Returns(job2);
            _containerMock.SetupResolveNamed(group2.First().PluginId.ToString().ToUpper(), releaseContentMock2.Object);
            var releaseContentMock3 = new Mock<IDeploymentTool>(MockBehavior.Strict);
            releaseContentMock3.Setup(x => x.GetReleaseJobs(It.Is<IEnumerable<Guid>>(ids => ids.All(id => group3.Any(p => p.PackageId == id)))))
                .Returns(job3);
            _containerMock.SetupResolveNamed(group3.First().PluginId.ToString().ToUpper(), releaseContentMock3.Object);

            var actual = Sut.GetReleaseJobs(packagePluginConfigurations.Select(x => x.PackageId));

            Assert.AreEqual(3, actual.Count());
            Assert.IsTrue(actual.Contains(job1.First()));
            Assert.IsTrue(actual.Contains(job2.First()));
            Assert.IsTrue(actual.Contains(job3.First()));

            _pluginGatewayMock.Verify(x => x.GetPackagePluginConfiguration(It.IsAny<Guid>(), It.IsAny<PluginType>()), Times.Exactly(packagePluginConfigurationCount));
            releaseContentMock1.Verify(x => x.GetReleaseJobs(It.IsAny<IEnumerable<Guid>>()), Times.Once);
            releaseContentMock2.Verify(x => x.GetReleaseJobs(It.IsAny<IEnumerable<Guid>>()), Times.Once);
            releaseContentMock3.Verify(x => x.GetReleaseJobs(It.IsAny<IEnumerable<Guid>>()), Times.Once);
        }

        [Test]
        public void GetReleaseJobs_ShouldReturnEmptyCollection_WhenPackagesDoesNotHaveReleaseContentPluginAssociated()
        {
            const int packagePluginConfigurationCount = 5;
            var packagePluginConfigurations = Builder<PackagePluginConfiguration>.CreateListOfSize(packagePluginConfigurationCount)
                .All()
                .Do(x => x.PluginId = null)
                .Do(x => x.PackageId = Guid.NewGuid())
                .Build();

            foreach (var packagePluginConfiguration in packagePluginConfigurations)
            {
                var configuration = packagePluginConfiguration;
                _pluginGatewayMock.Setup(x => x.GetPackagePluginConfiguration(configuration.PackageId, PluginType.DeploymentTool))
                    .Returns(packagePluginConfiguration);
            }
            _pluginGatewayMock.Setup(x => x.Dispose());

            var actual = Sut.GetReleaseJobs(packagePluginConfigurations.Select(x => x.PackageId));

            Assert.IsEmpty(actual);
            _pluginGatewayMock.Verify(x => x.GetPackagePluginConfiguration(It.IsAny<Guid>(), It.IsAny<PluginType>()), Times.Exactly(packagePluginConfigurationCount));
        }

        [Test]
        public void AllowGettingDeployTime_ShouldCallServiceMethod_WhenAssignedToThePackage()
        {
            var packageId = Guid.NewGuid();
            var deploymentToolMock = new Mock<IDeploymentTool>(MockBehavior.Strict);
            var pluginId = Guid.NewGuid();

            _containerMock.SetupResolveNamed(pluginId.ToString().ToUpper(), deploymentToolMock.Object);
            _pluginGatewayMock.Setup(x => x.GetPackagePluginConfiguration(packageId, PluginType.DeploymentTool))
                .Returns(new PackagePluginConfiguration { PluginId = pluginId });
            _pluginGatewayMock.Setup(x => x.Dispose());
            deploymentToolMock.Setup(x => x.AllowGettingDeployTime(packageId))
                .Returns(true);

            var result = Sut.AllowGettingDeployTime(packageId);

            Assert.AreEqual(true, result);

            deploymentToolMock.Verify(x => x.AllowGettingDeployTime(It.IsAny<Guid>()), Times.Once);
            _pluginGatewayMock.Verify(x => x.GetPackagePluginConfiguration(It.IsAny<Guid>(), It.IsAny<PluginType>()), Times.Once);
        }

        [Test]
        public void AllowGettingDeployTime_ShouldReturnFalse_WhenNotAssignedToThePackage()
        {
            var packageId = Guid.NewGuid();

            _pluginGatewayMock.Setup(x => x.GetPackagePluginConfiguration(packageId, PluginType.DeploymentTool))
                .Returns(new PackagePluginConfiguration());
            _pluginGatewayMock.Setup(x => x.Dispose());

            var result = Sut.AllowGettingDeployTime(packageId);

            Assert.AreEqual(false, result);

            _pluginGatewayMock.Verify(x => x.GetPackagePluginConfiguration(It.IsAny<Guid>(), It.IsAny<PluginType>()), Times.Once);
        }
    }
}
