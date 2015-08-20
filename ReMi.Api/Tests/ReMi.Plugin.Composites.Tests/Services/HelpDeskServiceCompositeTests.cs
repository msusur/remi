using Autofac;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Plugins;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Data;
using ReMi.Contracts.Plugins.Data.HelpDesk;
using ReMi.Contracts.Plugins.Services.HelpDesk;
using ReMi.DataAccess.BusinessEntityGateways.Plugins;
using ReMi.Plugin.Composites.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReMi.Plugin.Composites.Tests.Services
{
    public class HelpDeskServiceCompositeTests : TestClassFor<HelpDeskServiceComposite>
    {
        private Mock<IPluginGateway> _pluginGatewayMock;
        private Mock<IContainer> _containerMock;

        protected override HelpDeskServiceComposite ConstructSystemUnderTest()
        {
            return new HelpDeskServiceComposite
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
        public void CreateTicket_ShouldCreateHelpDeskTicket_WhenServiceFound()
        {
            var packagePluginConfiguration = Builder<PackagePluginConfiguration>.CreateNew()
                .With(x => x.PluginId, Guid.NewGuid())
                .With(x => x.PackageId, Guid.NewGuid())
                .Build();
            var ticket = Builder<HelpDeskTicket>.CreateNew().Build();
            var createdTicket = new HelpDeskTicket { Id = RandomData.RandomString(10) };

            _pluginGatewayMock.Setup(x => x.GetPackagePluginConfiguration(packagePluginConfiguration.PackageId, PluginType.HelpDesk))
                .Returns(packagePluginConfiguration);
            _pluginGatewayMock.Setup(x => x.Dispose());

            var helpDeskServiceMock = new Mock<IHelpDeskService>(MockBehavior.Strict);
            helpDeskServiceMock.Setup(x => x.CreateTicket(ticket,
                It.Is<IEnumerable<Guid>>(ids => ids.SequenceEqual(new[] { packagePluginConfiguration.PackageId }))))
                .Returns(createdTicket);
            _containerMock.SetupResolveNamed(packagePluginConfiguration.PluginId.ToString().ToUpper(), helpDeskServiceMock.Object);

            var result = Sut.CreateTicket(ticket, new[] { packagePluginConfiguration.PackageId });

            Assert.AreEqual(createdTicket, result);
            _pluginGatewayMock.Verify(x => x.GetPackagePluginConfiguration(It.IsAny<Guid>(), It.IsAny<PluginType>()), Times.Once);
            helpDeskServiceMock.Verify(x => x.CreateTicket(It.IsAny<HelpDeskTicket>(), It.IsAny<IEnumerable<Guid>>()), Times.Once);
        }

        [Test]
        public void UpdateTicket_ShouldUpdateHelpDeskTicket_WhenServiceFound()
        {
            var packagePluginConfiguration = Builder<PackagePluginConfiguration>.CreateNew()
                .With(x => x.PluginId, Guid.NewGuid())
                .With(x => x.PackageId, Guid.NewGuid())
                .Build();
            var ticket = Builder<HelpDeskTicket>.CreateNew().Build();
            var updatedTicket = new HelpDeskTicket { Id = RandomData.RandomString(10) };

            _pluginGatewayMock.Setup(x => x.GetPackagePluginConfiguration(packagePluginConfiguration.PackageId, PluginType.HelpDesk))
                .Returns(packagePluginConfiguration);
            _pluginGatewayMock.Setup(x => x.Dispose());

            var helpDeskServiceMock = new Mock<IHelpDeskService>(MockBehavior.Strict);
            helpDeskServiceMock.Setup(x => x.UpdateTicket(ticket,
                It.Is<IEnumerable<Guid>>(ids => ids.SequenceEqual(new[] { packagePluginConfiguration.PackageId }))))
                .Returns(updatedTicket);
            _containerMock.SetupResolveNamed(packagePluginConfiguration.PluginId.ToString().ToUpper(), helpDeskServiceMock.Object);

            var result = Sut.UpdateTicket(ticket, new[] { packagePluginConfiguration.PackageId });

            Assert.AreEqual(updatedTicket, result);
            _pluginGatewayMock.Verify(x => x.GetPackagePluginConfiguration(It.IsAny<Guid>(), It.IsAny<PluginType>()), Times.Once);
            helpDeskServiceMock.Verify(x => x.UpdateTicket(It.IsAny<HelpDeskTicket>(), It.IsAny<IEnumerable<Guid>>()), Times.Once);
        }

        [Test]
        public void DeleteTicket_ShouldUpdateHelpDeskTicket_WhenServiceFound()
        {
            var packagePluginConfiguration = Builder<PackagePluginConfiguration>.CreateNew()
                .With(x => x.PluginId, Guid.NewGuid())
                .With(x => x.PackageId, Guid.NewGuid())
                .Build();
            var ticket = Builder<HelpDeskTicket>.CreateNew().Build();

            _pluginGatewayMock.Setup(x => x.GetPackagePluginConfiguration(packagePluginConfiguration.PackageId, PluginType.HelpDesk))
                .Returns(packagePluginConfiguration);
            _pluginGatewayMock.Setup(x => x.Dispose());

            var helpDeskServiceMock = new Mock<IHelpDeskService>(MockBehavior.Strict);
            helpDeskServiceMock.Setup(x => x.DeleteTicket(ticket.Id,
                It.Is<IEnumerable<Guid>>(ids => ids.SequenceEqual(new[] {packagePluginConfiguration.PackageId}))));
            _containerMock.SetupResolveNamed(packagePluginConfiguration.PluginId.ToString().ToUpper(), helpDeskServiceMock.Object);

            Sut.DeleteTicket(ticket.Id, new[] { packagePluginConfiguration.PackageId });

            _pluginGatewayMock.Verify(x => x.GetPackagePluginConfiguration(It.IsAny<Guid>(), It.IsAny<PluginType>()), Times.Once);
            helpDeskServiceMock.Verify(x => x.DeleteTicket(It.IsAny<string>(), It.IsAny<IEnumerable<Guid>>()), Times.Once);
        }
    }
}
