using Moq;
using NUnit.Framework;
using ReMi.BusinessLogic.ReleaseExecution;
using ReMi.Common.Constants.ReleaseExecution;
using ReMi.TestUtils.UnitTests;
using ReMi.EventHandlers.ReleaseExecution;
using ReMi.Events.Metrics;

namespace ReMi.EventHandlers.Tests.ReleaseExecution
{
    public class SendNotificationOnSiteDownMetricsUpdatedEventHandlerTests : TestClassFor<SendNotificationOnSiteDownMetricsUpdatedEventHandler>
    {
        private Mock<IMetricsChangeEmailNotificationSender> _emailSenderMock;

        protected override SendNotificationOnSiteDownMetricsUpdatedEventHandler ConstructSystemUnderTest()
        {
            return new SendNotificationOnSiteDownMetricsUpdatedEventHandler
            {
                EmailSender = _emailSenderMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _emailSenderMock = new Mock<IMetricsChangeEmailNotificationSender>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldSendEmail_WhenHandled()
        {
            var evnt = new MetricsUpdatedEvent();

            _emailSenderMock.Setup(x => x.Send(evnt, MetricType.SiteDown, "{0} MAINTENANCE MODE ON (SITE OFFLINE)"));

            Sut.Handle(evnt);

            _emailSenderMock.Verify(x => x.Send(It.IsAny<MetricsUpdatedEvent>(), It.IsAny<MetricType>(), It.IsAny<string>()), Times.Once);
        }
    }
}
