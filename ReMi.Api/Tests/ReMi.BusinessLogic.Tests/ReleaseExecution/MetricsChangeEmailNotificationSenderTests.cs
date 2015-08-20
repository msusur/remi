using NUnit.Framework;
using ReMi.BusinessLogic.ReleaseExecution;
using ReMi.Common.Utils.UnitTests;
using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.Metrics;
using ReMi.Common.Constants.ReleaseExecution;
using ReMi.Common.Constants.Subscriptions;
using ReMi.Contracts.Plugins.Services.Email;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.Subscriptions;
using ReMi.Events.Metrics;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Common.Utils;

namespace ReMi.BusinessLogic.Tests.ReleaseExecution
{
    [TestFixture]
    public class MetricsChangeEmailNotificationSenderTests : TestClassFor<MetricsChangeEmailNotificationSender>
    {
        private Mock<IReleaseWindowGateway> _releaseWindowGatewayMock;
        private Mock<IAccountNotificationGateway> _accountNotificationGatewayMock;
        private Mock<IEmailTextProvider> _emailTextProviderMock;
        private Mock<IEmailService> _emailServiceMock;
        private Mock<IApplicationSettings> _applicationSettingsMock;

        protected override MetricsChangeEmailNotificationSender ConstructSystemUnderTest()
        {
            return new MetricsChangeEmailNotificationSender
            {
                ReleaseWindowGatewayFactory = () => _releaseWindowGatewayMock.Object,
                AccountNotificationGatewayFactory = () => _accountNotificationGatewayMock.Object,
                EmailService = _emailServiceMock.Object,
                EmailTextProvider = _emailTextProviderMock.Object,
                ApplicationSettings = _applicationSettingsMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseWindowGatewayMock = new Mock<IReleaseWindowGateway>(MockBehavior.Strict);
            _accountNotificationGatewayMock = new Mock<IAccountNotificationGateway>(MockBehavior.Strict);
            _emailTextProviderMock = new Mock<IEmailTextProvider>(MockBehavior.Strict);
            _emailServiceMock = new Mock<IEmailService>(MockBehavior.Strict);
            _applicationSettingsMock = new Mock<IApplicationSettings>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Send_ShouldDoNothing_WhenMetricTypeIsDifferentThenRequested()
        {
            var evt = new MetricsUpdatedEvent { Metric = new Metric { MetricType = MetricType.SiteDown } };
            const MetricType metricType = MetricType.SiteUp;

            Sut.Send(evt, metricType, null);

            _releaseWindowGatewayMock.Verify(x => x.GetByExternalId(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
            _accountNotificationGatewayMock.Verify(x => x.GetSubscribers(It.IsAny<NotificationType>()), Times.Never);
            _emailTextProviderMock.Verify(x => x.GetText(It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, object>>>()), Times.Never);
            _emailServiceMock.Verify(x => x.Send(It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void Send_ShouldDoNothing_SubscribersListIsEmpty()
        {
            var evt = new MetricsUpdatedEvent
            {
                Metric = new Metric { MetricType = MetricType.SiteDown },
                ReleaseWindowId = Guid.NewGuid()
            };
            const MetricType metricType = MetricType.SiteDown;
            const string product = "test prod";
            var releaseWindow = new ReleaseWindow
            {
                Products = new[] { product },
                ExternalId = evt.ReleaseWindowId
            };

            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(releaseWindow.ExternalId, true, false))
                .Returns(releaseWindow);
            _releaseWindowGatewayMock.Setup(x => x.Dispose());
            _accountNotificationGatewayMock.Setup(x => x.GetSubscribers(NotificationType.SiteDown, It.Is<IEnumerable<string>>(p => p.First() == product)))
                .Returns((List<Account>)null);
            _accountNotificationGatewayMock.Setup(x => x.Dispose());

            Sut.Send(evt, metricType, null);

            _releaseWindowGatewayMock.Verify(x => x.GetByExternalId(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
            _accountNotificationGatewayMock.Verify(x => x.GetSubscribers(It.IsAny<NotificationType>(), It.IsAny<IEnumerable<string>>()), Times.Once);
            _emailTextProviderMock.Verify(x => x.GetText(It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, object>>>()), Times.Never);
            _emailServiceMock.Verify(x => x.Send(It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void Send_ShouldSendEmailToAllSubscribers_WhenSuccess()
        {
            var evt = new MetricsUpdatedEvent
            {
                Metric = new Metric { MetricType = MetricType.SiteDown },
                ReleaseWindowId = Guid.NewGuid()
            };
            const MetricType metricType = MetricType.SiteDown;
            const string product = "test prod";
            var releaseWindow = new ReleaseWindow
            {
                Products = new[] { product },
                ExternalId = evt.ReleaseWindowId
            };
            var subscribers = Builder<Account>.CreateListOfSize(2).Build();
            const string emailSubjectTemplate = "{0} subject";
            const string email1 = "email1";
            const string email2 = "email2";

            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(releaseWindow.ExternalId, true, false))
                .Returns(releaseWindow);
            _releaseWindowGatewayMock.Setup(x => x.Dispose());
            _accountNotificationGatewayMock.Setup(x => x.GetSubscribers(NotificationType.SiteDown, It.Is<IEnumerable<string>>(p => p.First() == product)))
                .Returns(subscribers);
            _accountNotificationGatewayMock.Setup(x => x.Dispose());
            _applicationSettingsMock.SetupGet(x => x.FrontEndUrl).Returns((string) null);
            _emailTextProviderMock.Setup(x => x.GetText(metricType.ToString(), It.Is<IDictionary<string, object>>(v =>
                v["Recipient"].ToString() == subscribers[0].FullName)))
                .Returns(email1);
            _emailTextProviderMock.Setup(x => x.GetText(metricType.ToString(), It.Is<IDictionary<string, object>>(v =>
                v["Recipient"].ToString() == subscribers[1].FullName)))
                .Returns(email2);
            _emailServiceMock.Setup(x => x.Send(subscribers[0].Email, "test prod subject", email1));
            _emailServiceMock.Setup(x => x.Send(subscribers[1].Email, "test prod subject", email2));

            Sut.Send(evt, metricType, emailSubjectTemplate);

            _releaseWindowGatewayMock.Verify(x => x.GetByExternalId(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
            _accountNotificationGatewayMock.Verify(x => x.GetSubscribers(It.IsAny<NotificationType>(), It.IsAny<IEnumerable<string>>()), Times.Once);
            _emailTextProviderMock.Verify(x => x.GetText(It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, object>>>()), Times.Exactly(2));
            _emailServiceMock.Verify(x => x.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
            _applicationSettingsMock.VerifyGet(x => x.FrontEndUrl, Times.Exactly(2));
        }
    }
}
