using Autofac;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Plugins;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Data;
using ReMi.Contracts.Plugins.Data.Email;
using ReMi.Contracts.Plugins.Services.Email;
using ReMi.DataAccess.BusinessEntityGateways.Plugins;
using ReMi.Plugin.Composites.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReMi.Plugin.Composites.Tests.Services
{
    public class EmailServiceCompositeTests : TestClassFor<EmailServiceComposite>
    {
        private Mock<IPluginGateway> _pluginGatewayMock;
        private Mock<IContainer> _containerMock;

        protected override EmailServiceComposite ConstructSystemUnderTest()
        {
            return new EmailServiceComposite
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
        public void Send_ShouldReturnNull_WhenPluginTypeNotFound()
        {
            var packageConfiguration = new GlobalPluginConfiguration
            {
                ExternalId = Guid.NewGuid(),
                PluginType = PluginType.Authentication
            };
            var email = RandomData.RandomEmail();
            var subject = RandomData.RandomString(10);
            var body = RandomData.RandomString(20);

            _pluginGatewayMock.Setup(x => x.GetGlobalPluginConfiguration())
                .Returns(new[] { packageConfiguration });
            _pluginGatewayMock.Setup(x => x.Dispose());

            Sut.Send(email, subject, body);

            _pluginGatewayMock.Verify(x => x.GetGlobalPluginConfiguration(), Times.Once);
            _pluginGatewayMock.Verify(x => x.Dispose(), Times.Once);
        }

        [Test]
        public void Send_ShouldReturnNull_WhenServiceNotAssignedToPlugin()
        {
            var packageConfiguration = new GlobalPluginConfiguration
            {
                ExternalId = Guid.NewGuid(),
                PluginType = PluginType.Email
            };
            var email = RandomData.RandomEmail();
            var subject = RandomData.RandomString(10);
            var body = RandomData.RandomString(20);

            _pluginGatewayMock.Setup(x => x.GetGlobalPluginConfiguration())
                .Returns(new[] { packageConfiguration });
            _pluginGatewayMock.Setup(x => x.Dispose());

            Sut.Send(email, subject, body);

            _pluginGatewayMock.Verify(x => x.GetGlobalPluginConfiguration(), Times.Once);
            _pluginGatewayMock.Verify(x => x.Dispose(), Times.Once);
        }

        [Test]
        public void Send_ShouldCallService_WhenServiceFound()
        {
            var packageConfiguration = new GlobalPluginConfiguration
            {
                ExternalId = Guid.NewGuid(),
                PluginType = PluginType.Email,
                PluginId = Guid.NewGuid()
            };
            var email = RandomData.RandomEmail();
            var subject = RandomData.RandomString(10);
            var body = RandomData.RandomString(20);
            var service = new Mock<IEmailService>(MockBehavior.Strict);

            _pluginGatewayMock.Setup(x => x.GetGlobalPluginConfiguration())
                .Returns(new[] { packageConfiguration });
            _pluginGatewayMock.Setup(x => x.Dispose());
            _containerMock.SetupResolveNamed(packageConfiguration.PluginId.ToString().ToUpper(), service.Object);
            service.Setup(x => x.Send(email, subject, body));

            Sut.Send(email, subject, body);

            _pluginGatewayMock.Verify(x => x.GetGlobalPluginConfiguration(), Times.Once);
            _pluginGatewayMock.Verify(x => x.Dispose(), Times.Once);
            service.Verify(x => x.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void Send_ShouldCallServiceWithMultipleEmailAddresses_WhenServiceFound()
        {
            var packageConfiguration = new GlobalPluginConfiguration
            {
                ExternalId = Guid.NewGuid(),
                PluginType = PluginType.Email,
                PluginId = Guid.NewGuid()
            };
            var emails = Enumerable.Repeat(RandomData.RandomEmail(), 5);
            var subject = RandomData.RandomString(10);
            var body = RandomData.RandomString(20);
            var service = new Mock<IEmailService>(MockBehavior.Strict);

            _pluginGatewayMock.Setup(x => x.GetGlobalPluginConfiguration())
                .Returns(new[] { packageConfiguration });
            _pluginGatewayMock.Setup(x => x.Dispose());
            _containerMock.SetupResolveNamed(packageConfiguration.PluginId.ToString().ToUpper(), service.Object);
            service.Setup(x => x.Send(emails, subject, body));

            Sut.Send(emails, subject, body);

            _pluginGatewayMock.Verify(x => x.GetGlobalPluginConfiguration(), Times.Once);
            _pluginGatewayMock.Verify(x => x.Dispose(), Times.Once);
            service.Verify(x => x.Send(It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void SendWithCalendarEvent_ShouldCallService_WhenServiceFound()
        {
            var packageConfiguration = new GlobalPluginConfiguration
            {
                ExternalId = Guid.NewGuid(),
                PluginType = PluginType.Email,
                PluginId = Guid.NewGuid()
            };
            var email = RandomData.RandomEmail();
            var subject = RandomData.RandomString(10);
            var body = RandomData.RandomString(20);
            var calendarEvent = new CalendarEvent();
            const int releaseDuration = 10;
            var service = new Mock<IEmailService>(MockBehavior.Strict);

            _pluginGatewayMock.Setup(x => x.GetGlobalPluginConfiguration())
                .Returns(new[] { packageConfiguration });
            _pluginGatewayMock.Setup(x => x.Dispose());
            _containerMock.SetupResolveNamed(packageConfiguration.PluginId.ToString().ToUpper(), service.Object);
            service.Setup(x => x.SendWithCalendarEvent(email, subject, body, calendarEvent, releaseDuration));

            Sut.SendWithCalendarEvent(email, subject, body, calendarEvent, releaseDuration);

            _pluginGatewayMock.Verify(x => x.GetGlobalPluginConfiguration(), Times.Once);
            _pluginGatewayMock.Verify(x => x.Dispose(), Times.Once);
            service.Verify(x => x.SendWithCalendarEvent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CalendarEvent>(), It.IsAny<int>()), Times.Once);
        }
    }
}
