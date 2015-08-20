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
using ReMi.Contracts.Plugins.Data.ReleaseContent;
using ReMi.Contracts.Plugins.Services.ReleaseContent;
using ReMi.DataAccess.BusinessEntityGateways.Plugins;
using ReMi.Plugin.Composites.Services;

namespace ReMi.Plugin.Composites.Tests.Services
{
    public class ReleaseContentCompositeTests : TestClassFor<ReleaseContentComposite>
    {
        private Mock<IPluginGateway> _pluginGatewayMock;
        private Mock<IContainer> _containerMock;

        protected override ReleaseContentComposite ConstructSystemUnderTest()
        {
            return new ReleaseContentComposite
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
        public void UpdateTicket_ShouldUpdateTicketProperPlugin_WhenPluginIsAssignToThePackage()
        {
            var packagePluginConfiguration = Builder<PackagePluginConfiguration>.CreateNew()
                .With(x => x.PluginId, Guid.NewGuid())
                .With(x => x.PackageId, Guid.NewGuid())
                .Build();
            var tickets = Enumerable.Empty<ReleaseContentTicket>();

            _pluginGatewayMock.Setup(x => x.GetPackagePluginConfiguration(packagePluginConfiguration.PackageId, PluginType.ReleaseContent))
                .Returns(packagePluginConfiguration);
            _pluginGatewayMock.Setup(x => x.Dispose());

            var releaseContentMock = new Mock<IReleaseContent>(MockBehavior.Strict);
            releaseContentMock.Setup(x => x.UpdateTicket(tickets, packagePluginConfiguration.PackageId));
            _containerMock.SetupResolveNamed(packagePluginConfiguration.PluginId.ToString().ToUpper(), releaseContentMock.Object);

            Sut.UpdateTicket(tickets, packagePluginConfiguration.PackageId);

            _pluginGatewayMock.Verify(x => x.GetPackagePluginConfiguration(It.IsAny<Guid>(), It.IsAny<PluginType>()), Times.Once);
            releaseContentMock.Verify(x => x.UpdateTicket(It.IsAny<IEnumerable<ReleaseContentTicket>>(), It.IsAny<Guid>()), Times.Once);
        }

        [Test]
        public void GetTickets_ShouldReturnTicketsForManyPackages_WhenListOfPackagesAreAssignedToMoreThenOnePlugin()
        {
            const int packagePluginConfigurationCount = 5;
            var tickets1 = Builder<ReleaseContentTicket>.CreateListOfSize(1).Build();
            var tickets2 = Builder<ReleaseContentTicket>.CreateListOfSize(1).Build();
            var tickets3 = Builder<ReleaseContentTicket>.CreateListOfSize(1).Build();
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
                _pluginGatewayMock.Setup(x => x.GetPackagePluginConfiguration(configuration.PackageId, PluginType.ReleaseContent))
                    .Returns(packagePluginConfiguration);
            }
            _pluginGatewayMock.Setup(x => x.Dispose());

            var releaseContentMock1 = new Mock<IReleaseContent>(MockBehavior.Strict);
            releaseContentMock1.Setup(x => x.GetTickets(It.Is<IEnumerable<Guid>>(ids => ids.All(id => group1.Any(p => p.PackageId == id)))))
                .Returns(tickets1);
            _containerMock.SetupResolveNamed(group1.First().PluginId.ToString().ToUpper(), releaseContentMock1.Object);
            var releaseContentMock2 = new Mock<IReleaseContent>(MockBehavior.Strict);
            releaseContentMock2.Setup(x => x.GetTickets(It.Is<IEnumerable<Guid>>(ids => ids.All(id => group2.Any(p => p.PackageId == id)))))
                .Returns(tickets2);
            _containerMock.SetupResolveNamed(group2.First().PluginId.ToString().ToUpper(), releaseContentMock2.Object);
            var releaseContentMock3 = new Mock<IReleaseContent>(MockBehavior.Strict);
            releaseContentMock3.Setup(x => x.GetTickets(It.Is<IEnumerable<Guid>>(ids => ids.All(id => group3.Any(p => p.PackageId == id)))))
                .Returns(tickets3);
            _containerMock.SetupResolveNamed(group3.First().PluginId.ToString().ToUpper(), releaseContentMock3.Object);

            var actual = Sut.GetTickets(packagePluginConfigurations.Select(x => x.PackageId));

            Assert.AreEqual(3, actual.Count());
            Assert.IsTrue(actual.Contains(tickets1.First()));
            Assert.IsTrue(actual.Contains(tickets2.First()));
            Assert.IsTrue(actual.Contains(tickets3.First()));

            _pluginGatewayMock.Verify(x => x.GetPackagePluginConfiguration(It.IsAny<Guid>(), It.IsAny<PluginType>()), Times.Exactly(packagePluginConfigurationCount));
            releaseContentMock1.Verify(x => x.GetTickets(It.IsAny<IEnumerable<Guid>>()), Times.Once);
            releaseContentMock2.Verify(x => x.GetTickets(It.IsAny<IEnumerable<Guid>>()), Times.Once);
            releaseContentMock3.Verify(x => x.GetTickets(It.IsAny<IEnumerable<Guid>>()), Times.Once);
        }

        [Test]
        public void GetTickets_ShouldReturnEmptyCollection_WhenPackagesDoesnotHaveReleaseContentPluginAssociated()
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
                _pluginGatewayMock.Setup(x => x.GetPackagePluginConfiguration(configuration.PackageId, PluginType.ReleaseContent))
                    .Returns(packagePluginConfiguration);
            }
            _pluginGatewayMock.Setup(x => x.Dispose());

            var actual = Sut.GetTickets(packagePluginConfigurations.Select(x => x.PackageId));

            Assert.IsEmpty(actual);
            _pluginGatewayMock.Verify(x => x.GetPackagePluginConfiguration(It.IsAny<Guid>(), It.IsAny<PluginType>()), Times.Exactly(packagePluginConfigurationCount));
        }

        [Test]
        public void GetDefectTickets_ShouldReturnTicketsForManyPackages_WhenListOfPackagesAreAssignedToMoreThenOnePlugin()
        {
            const int packagePluginConfigurationCount = 5;
            var tickets1 = Builder<ReleaseContentTicket>.CreateListOfSize(1).Build();
            var tickets2 = Builder<ReleaseContentTicket>.CreateListOfSize(1).Build();
            var tickets3 = Builder<ReleaseContentTicket>.CreateListOfSize(1).Build();
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
                _pluginGatewayMock.Setup(x => x.GetPackagePluginConfiguration(configuration.PackageId, PluginType.ReleaseContent))
                    .Returns(packagePluginConfiguration);
            }
            _pluginGatewayMock.Setup(x => x.Dispose());

            var releaseContentMock1 = new Mock<IReleaseContent>(MockBehavior.Strict);
            releaseContentMock1.Setup(x => x.GetDefectTickets(It.Is<IEnumerable<Guid>>(ids => ids.All(id => group1.Any(p => p.PackageId == id)))))
                .Returns(tickets1);
            _containerMock.SetupResolveNamed(group1.First().PluginId.ToString().ToUpper(), releaseContentMock1.Object);
            var releaseContentMock2 = new Mock<IReleaseContent>(MockBehavior.Strict);
            releaseContentMock2.Setup(x => x.GetDefectTickets(It.Is<IEnumerable<Guid>>(ids => ids.All(id => group2.Any(p => p.PackageId == id)))))
                .Returns(tickets2);
            _containerMock.SetupResolveNamed(group2.First().PluginId.ToString().ToUpper(), releaseContentMock2.Object);
            var releaseContentMock3 = new Mock<IReleaseContent>(MockBehavior.Strict);
            releaseContentMock3.Setup(x => x.GetDefectTickets(It.Is<IEnumerable<Guid>>(ids => ids.All(id => group3.Any(p => p.PackageId == id)))))
                .Returns(tickets3);
            _containerMock.SetupResolveNamed(group3.First().PluginId.ToString().ToUpper(), releaseContentMock3.Object);

            var actual = Sut.GetDefectTickets(packagePluginConfigurations.Select(x => x.PackageId));

            Assert.AreEqual(3, actual.Count());
            Assert.IsTrue(actual.Contains(tickets1.First()));
            Assert.IsTrue(actual.Contains(tickets2.First()));
            Assert.IsTrue(actual.Contains(tickets3.First()));

            _pluginGatewayMock.Verify(x => x.GetPackagePluginConfiguration(It.IsAny<Guid>(), It.IsAny<PluginType>()), Times.Exactly(packagePluginConfigurationCount));
            releaseContentMock1.Verify(x => x.GetDefectTickets(It.IsAny<IEnumerable<Guid>>()), Times.Once);
            releaseContentMock2.Verify(x => x.GetDefectTickets(It.IsAny<IEnumerable<Guid>>()), Times.Once);
            releaseContentMock3.Verify(x => x.GetDefectTickets(It.IsAny<IEnumerable<Guid>>()), Times.Once);
        }

        [Test]
        public void GetDefectTickets_ShouldReturnEmptyCollection_WhenPackagesDoesnotHaveReleaseContentPluginAssociated()
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
                _pluginGatewayMock.Setup(x => x.GetPackagePluginConfiguration(configuration.PackageId, PluginType.ReleaseContent))
                    .Returns(packagePluginConfiguration);
            }
            _pluginGatewayMock.Setup(x => x.Dispose());

            var actual = Sut.GetDefectTickets(packagePluginConfigurations.Select(x => x.PackageId));

            Assert.IsEmpty(actual);
            _pluginGatewayMock.Verify(x => x.GetPackagePluginConfiguration(It.IsAny<Guid>(), It.IsAny<PluginType>()), Times.Exactly(packagePluginConfigurationCount));
        }
    }
}
