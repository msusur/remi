using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Plugins;
using ReMi.Common.Utils.Repository;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Data;
using ReMi.DataAccess.BusinessEntityGateways.Plugins;
using ReMi.DataEntities.Plugins;
using ReMi.DataEntities.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.DataAccess.Exceptions;
using Plugin = ReMi.DataEntities.Plugins.Plugin;

namespace ReMi.DataAccess.Tests.Plugins
{
    [TestFixture]
    public class PluginGatewayTests : TestClassFor<PluginGateway>
    {
        private Mock<IRepository<Product>> _productRepositoryMock;
        private Mock<IRepository<PluginConfiguration>> _pluginConfigurationRepositoryMock;
        private Mock<IRepository<PluginPackageConfiguration>> _pluginPackageConfigurationRepositoryMock;
        private Mock<IRepository<DataEntities.Plugins.Plugin>> _pluginRepositoryMock;
        private Mock<IMappingEngine> _mappingEngineMock;

        protected override PluginGateway ConstructSystemUnderTest()
        {
            _productRepositoryMock = new Mock<IRepository<Product>>(MockBehavior.Strict);
            _pluginConfigurationRepositoryMock =
                new Mock<IRepository<PluginConfiguration>>(MockBehavior.Strict);
            _pluginPackageConfigurationRepositoryMock =
                new Mock<IRepository<PluginPackageConfiguration>>(MockBehavior.Strict);
            _pluginRepositoryMock =
                new Mock<IRepository<DataEntities.Plugins.Plugin>>(MockBehavior.Strict);
            _mappingEngineMock = new Mock<IMappingEngine>(MockBehavior.Strict);

            return new PluginGateway
            {
                ProductRepository = _productRepositoryMock.Object,
                PluginConfigurationRepository = _pluginConfigurationRepositoryMock.Object,
                PluginPackageConfigurationRepository = _pluginPackageConfigurationRepositoryMock.Object,
                PluginRepository = _pluginRepositoryMock.Object,
                Mapper = _mappingEngineMock.Object
            };
        }

        [Test]
        public void AddPluginPackageConfiguration_ShouldAddNewConfigurationForPackage_WhenCalled()
        {
            var package = new Product
            {
                ExternalId = Guid.NewGuid(),
                ProductId = RandomData.RandomInt(int.MaxValue)
            };

            _productRepositoryMock.SetupEntities(new[] { package });
            _pluginPackageConfigurationRepositoryMock.SetupEntities(new[]
            {
                new PluginPackageConfiguration { PluginType = PluginType.QaStats },
                new PluginPackageConfiguration { PluginType = PluginType.ReleaseContent },
            });
            _pluginPackageConfigurationRepositoryMock.Setup(x => x.Insert(It.Is<IEnumerable<PluginPackageConfiguration>>(
                c => c.All(p => p.PackageId == package.ProductId)
                    && c.Count() == 2
                    && c.Count(p => p.PluginType == PluginType.QaStats) == 1
                    && c.Count(p => p.PluginType == PluginType.ReleaseContent) == 1)));

            Sut.AddPluginPackageConfiguration(package.ExternalId);

            _pluginPackageConfigurationRepositoryMock.Verify(x => x.Insert(It.IsAny<IEnumerable<PluginPackageConfiguration>>()), Times.Once);
        }

        [Test]
        public void GetGlobalPluginConfiguration_ShouldReturnListOfGlobalPlugins_WhenCalled()
        {
            var globalPlugins = Builder<PluginConfiguration>.CreateListOfSize(5).Build();
            var businessGlobalPlugins = Builder<GlobalPluginConfiguration>.CreateListOfSize(5).Build();

            _pluginConfigurationRepositoryMock.SetupEntities(globalPlugins);
            _mappingEngineMock.Setup(
                x => x.Map<IEnumerable<PluginConfiguration>, IEnumerable<GlobalPluginConfiguration>>(globalPlugins))
                .Returns(businessGlobalPlugins);

            var actual = Sut.GetGlobalPluginConfiguration();

            Assert.AreEqual(businessGlobalPlugins, actual);
            _mappingEngineMock.Verify(
                x => x.Map<IEnumerable<PluginConfiguration>, IEnumerable<GlobalPluginConfiguration>>(
                    It.IsAny<IEnumerable<PluginConfiguration>>()), Times.Once);
        }

        [Test]
        public void GetPackagePluginConfiguration_ShouldReturnListOfPackagePlugins_WhenCalled()
        {
            var packagePlugins = Builder<PluginPackageConfiguration>.CreateListOfSize(5)
                .All()
                .Do(x => x.Package = new Product
                {
                    BusinessUnit = new BusinessUnit { Name = RandomData.RandomString(10) },
                    Description = RandomData.RandomString(10)
                })
                .Build();
            var businessPackagePlugins = Builder<PackagePluginConfiguration>.CreateListOfSize(5).Build();

            _pluginPackageConfigurationRepositoryMock.SetupEntities(packagePlugins);
            _mappingEngineMock.Setup(x => x.Map<IEnumerable<PluginPackageConfiguration>, IEnumerable<PackagePluginConfiguration>>(
                It.Is<IEnumerable<PluginPackageConfiguration>>(p => p.Count() == packagePlugins.Count)))
                .Returns(businessPackagePlugins);

            var actual = Sut.GetPackagePluginConfiguration();

            Assert.AreEqual(businessPackagePlugins, actual);
            _mappingEngineMock.Verify(x => x.Map<IEnumerable<PluginPackageConfiguration>, IEnumerable<PackagePluginConfiguration>>(
                It.IsAny<IEnumerable<PluginPackageConfiguration>>()), Times.Once);
        }

        [Test]
        public void GetPackagePluginConfiguration_ShouldReturnPackagePlugin_WhenCalled()
        {
            var packagePlugins = Builder<PluginPackageConfiguration>.CreateListOfSize(5)
                .All()
                .Do(x => x.Package = new Product
                {
                    BusinessUnit = new BusinessUnit { Name = RandomData.RandomString(10) },
                    Description = RandomData.RandomString(10)
                })
                .Build();
            var packagePlugin = packagePlugins.ElementAt(2);
            var businessPackagePlugin = Builder<PackagePluginConfiguration>.CreateNew()
                .With(x => x.ExternalId, packagePlugin.ExternalId)
                .Build();

            _pluginPackageConfigurationRepositoryMock.SetupEntities(packagePlugins);
            _mappingEngineMock.Setup(x => x.Map<PluginPackageConfiguration, PackagePluginConfiguration>(packagePlugin))
                .Returns(businessPackagePlugin);

            var actual = Sut.GetPackagePluginConfiguration(packagePlugin.Package.ExternalId, packagePlugin.PluginType);

            Assert.AreEqual(businessPackagePlugin, actual);
            _mappingEngineMock.Verify(x => x.Map<PluginPackageConfiguration, PackagePluginConfiguration>(
                It.IsAny<PluginPackageConfiguration>()), Times.Once);
        }

        [Test]
        public void AssignGlobalPlugin_ShouldThrowEntityNotFound_WhenConfigurationNotFound()
        {
            _pluginConfigurationRepositoryMock.SetupEntities(Enumerable.Empty<PluginConfiguration>());
            var ex = Assert.Throws<EntityNotFoundException>(() => Sut.AssignGlobalPlugin(Guid.NewGuid(), null));
            Assert.AreEqual(typeof(PluginConfiguration).Name, ex.EntityType);
        }

        [Test]
        public void AssignGlobalPlugin_ShouldThrowEntityNotFound_WhenPluginIdIsNotEmptyAndNotFound()
        {
            var configuration = Builder<PluginConfiguration>.CreateListOfSize(5)
                .All()
                .Do(x => x.ExternalId = Guid.NewGuid())
                .Build();
            _pluginConfigurationRepositoryMock.SetupEntities(configuration);
            _pluginRepositoryMock.SetupEntities(Enumerable.Empty<DataEntities.Plugins.Plugin>());
            var ex = Assert.Throws<EntityNotFoundException>(() => Sut.AssignGlobalPlugin(configuration.First().ExternalId, Guid.NewGuid()));
            Assert.AreEqual(typeof(DataEntities.Plugins.Plugin).Name, ex.EntityType);
        }

        [Test]
        public void AssignGlobalPlugin_ShouldAssignEmptyPluginId_WhenPluginIdIsEmpty()
        {
            var configuration = Builder<PluginConfiguration>.CreateListOfSize(5)
                .All()
                .Do(x => x.ExternalId = Guid.NewGuid())
                .Build();
            var firstConfig = configuration.First();
            firstConfig.PluginId = 1;

            _pluginConfigurationRepositoryMock.SetupEntities(configuration);
            _pluginRepositoryMock.SetupEntities(Enumerable.Empty<DataEntities.Plugins.Plugin>());
            _pluginConfigurationRepositoryMock.Setup(x => x.Update(firstConfig))
                .Returns((ChangedFields<PluginConfiguration>)null);

            Sut.AssignGlobalPlugin(firstConfig.ExternalId, null);

            _pluginConfigurationRepositoryMock.Verify(x => x.Update(It.IsAny<PluginConfiguration>()), Times.Once);
            Assert.IsFalse(firstConfig.PluginId.HasValue);
        }

        [Test]
        public void AssignGlobalPlugin_ShouldAssignPluginId_WhenPluginIdIsNotEmpty()
        {
            var configuration = Builder<PluginConfiguration>.CreateListOfSize(5)
                .All()
                .Do(x => x.ExternalId = Guid.NewGuid())
                .Build();
            var firstConfig = configuration.First();
            firstConfig.PluginId = RandomData.RandomInt(1, 100);
            var plugins = Builder<DataEntities.Plugins.Plugin>.CreateListOfSize(5).Build();
            var firstPlugin = plugins.First();
            firstPlugin.PluginId = RandomData.RandomInt(101, 200);
            firstPlugin.ExternalId = Guid.NewGuid();

            _pluginConfigurationRepositoryMock.SetupEntities(configuration);
            _pluginRepositoryMock.SetupEntities(plugins);
            _pluginConfigurationRepositoryMock.Setup(x => x.Update(firstConfig))
                .Returns((ChangedFields<PluginConfiguration>)null);

            Sut.AssignGlobalPlugin(firstConfig.ExternalId, firstPlugin.ExternalId);

            _pluginConfigurationRepositoryMock.Verify(x => x.Update(It.IsAny<PluginConfiguration>()), Times.Once);
            Assert.AreEqual(firstPlugin.PluginId, firstConfig.PluginId);
        }

        [Test]
        public void AssignPackagePlugin_ShouldThrowEntityNotFound_WhenConfigurationNotFound()
        {
            _pluginPackageConfigurationRepositoryMock.SetupEntities(Enumerable.Empty<PluginPackageConfiguration>());
            var ex = Assert.Throws<EntityNotFoundException>(() => Sut.AssignPackagePlugin(Guid.NewGuid(), null));
            Assert.AreEqual(typeof(PluginPackageConfiguration).Name, ex.EntityType);
        }

        [Test]
        public void AssignPackagePlugin_ShouldThrowEntityNotFound_WhenPluginIdIsNotEmptyAndNotFound()
        {
            var configuration = Builder<PluginPackageConfiguration>.CreateListOfSize(5)
                .All()
                .Do(x => x.ExternalId = Guid.NewGuid())
                .Build();
            _pluginPackageConfigurationRepositoryMock.SetupEntities(configuration);
            _pluginRepositoryMock.SetupEntities(Enumerable.Empty<DataEntities.Plugins.Plugin>());
            var ex = Assert.Throws<EntityNotFoundException>(() => Sut.AssignPackagePlugin(configuration.First().ExternalId, Guid.NewGuid()));
            Assert.AreEqual(typeof(DataEntities.Plugins.Plugin).Name, ex.EntityType);
        }

        [Test]
        public void AssignPackagePlugin_ShouldAssignEmptyPluginId_WhenPluginIdIsEmpty()
        {
            var configuration = Builder<PluginPackageConfiguration>.CreateListOfSize(5)
                .All()
                .Do(x => x.ExternalId = Guid.NewGuid())
                .Build();
            var firstConfig = configuration.First();
            firstConfig.PluginId = 1;

            _pluginPackageConfigurationRepositoryMock.SetupEntities(configuration);
            _pluginRepositoryMock.SetupEntities(Enumerable.Empty<DataEntities.Plugins.Plugin>());
            _pluginPackageConfigurationRepositoryMock.Setup(x => x.Update(firstConfig))
                .Returns((ChangedFields<PluginPackageConfiguration>)null);

            Sut.AssignPackagePlugin(firstConfig.ExternalId, null);

            _pluginPackageConfigurationRepositoryMock.Verify(x => x.Update(It.IsAny<PluginPackageConfiguration>()), Times.Once);
            Assert.IsFalse(firstConfig.PluginId.HasValue);
        }

        [Test]
        public void AssignPackagePlugin_ShouldAssignPluginId_WhenPluginIdIsNotEmpty()
        {
            var configuration = Builder<PluginPackageConfiguration>.CreateListOfSize(5)
                .All()
                .Do(x => x.ExternalId = Guid.NewGuid())
                .Build();
            var firstConfig = configuration.First();
            firstConfig.PluginId = RandomData.RandomInt(1, 100);
            var plugins = Builder<DataEntities.Plugins.Plugin>.CreateListOfSize(5).Build();
            var firstPlugin = plugins.First();
            firstPlugin.PluginId = RandomData.RandomInt(101, 200);
            firstPlugin.ExternalId = Guid.NewGuid();

            _pluginPackageConfigurationRepositoryMock.SetupEntities(configuration);
            _pluginRepositoryMock.SetupEntities(plugins);
            _pluginPackageConfigurationRepositoryMock.Setup(x => x.Update(firstConfig))
                .Returns((ChangedFields<PluginPackageConfiguration>)null);

            Sut.AssignPackagePlugin(firstConfig.ExternalId, firstPlugin.ExternalId);

            _pluginPackageConfigurationRepositoryMock.Verify(x => x.Update(It.IsAny<PluginPackageConfiguration>()), Times.Once);
            Assert.AreEqual(firstPlugin.PluginId, firstConfig.PluginId);
        }

        [Test]
        public void GetPlugins_ShouldReturnListOfPlugins_WhenCalled()
        {
            var plugins = Builder<DataEntities.Plugins.Plugin>.CreateListOfSize(5).Build();
            var businessPlugins = Builder<BusinessEntities.Plugins.Plugin>.CreateListOfSize(5).Build();

            _pluginRepositoryMock.SetupEntities(plugins);
            _mappingEngineMock.Setup(
                x => x.Map<IEnumerable<DataEntities.Plugins.Plugin>, IEnumerable<BusinessEntities.Plugins.Plugin>>(plugins))
                .Returns(businessPlugins);

            var actual = Sut.GetPlugins();

            Assert.AreEqual(businessPlugins, actual);
            _mappingEngineMock.Verify(
                x => x.Map<IEnumerable<DataEntities.Plugins.Plugin>, IEnumerable<BusinessEntities.Plugins.Plugin>>(
                    It.IsAny<IEnumerable<DataEntities.Plugins.Plugin>>()), Times.Once);
        }

        [Test]
        public void GetPlugin_ShouldReturnPluginByKeyName_WhenCalled()
        {
            var plugins = Builder<DataEntities.Plugins.Plugin>.CreateListOfSize(5).Build();
            var businessPlugin = Builder<BusinessEntities.Plugins.Plugin>.CreateNew().Build();

            _pluginRepositoryMock.SetupEntities(plugins);
            _mappingEngineMock.Setup(
                x => x.Map<DataEntities.Plugins.Plugin, BusinessEntities.Plugins.Plugin>(plugins[2]))
                .Returns(businessPlugin);

            var actual = Sut.GetPlugin(plugins[2].Key);

            Assert.AreEqual(businessPlugin, actual);
            _mappingEngineMock.Verify(
                x => x.Map<DataEntities.Plugins.Plugin, BusinessEntities.Plugins.Plugin>(
                    It.IsAny<DataEntities.Plugins.Plugin>()), Times.Once);
        }

        [Test]
        public void GetPlugin_ShouldThrowException_WhenPluginNotExists()
        {
            var plugins = Builder<DataEntities.Plugins.Plugin>.CreateListOfSize(5).Build();
            var key = RandomData.RandomString(10);

            _pluginRepositoryMock.SetupEntities(plugins);

            var ex = Assert.Throws<EntityNotFoundException>(() => Sut.GetPlugin(key));

            Assert.AreEqual(typeof(DataEntities.Plugins.Plugin).Name, ex.EntityType);
            Assert.AreEqual(string.Format("Could not find entity '{0}' of type 'Plugin'", key), ex.Message);
            _mappingEngineMock.Verify(
                x => x.Map<DataEntities.Plugins.Plugin, BusinessEntities.Plugins.Plugin>(
                    It.IsAny<DataEntities.Plugins.Plugin>()), Times.Never);
        }
    }
}
