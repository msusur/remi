using Moq;
using NUnit.Framework;
using ReMi.BusinessLogic.ReleaseExecution;
using ReMi.Common.Constants.ReleaseExecution;
using ReMi.TestUtils.UnitTests;
using ReMi.EventHandlers.ReleaseExecution;
using ReMi.Events.Metrics;

namespace ReMi.EventHandlers.Tests.ReleaseExecution
{
    public class SendNotificationOnSiteUpMetricsUpdatedEventHandlerTests : TestClassFor<SendNotificationOnSiteUpMetricsUpdatedEventHandler>
    {
        private Mock<IMetricsChangeEmailNotificationSender> _emailSenderMock;

        protected override SendNotificationOnSiteUpMetricsUpdatedEventHandler ConstructSystemUnderTest()
        {
            return new SendNotificationOnSiteUpMetricsUpdatedEventHandler
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

            _emailSenderMock.Setup(x => x.Send(evnt, MetricType.SiteUp, "{0} MAINTENANCE MODE OFF (SITE ONLINE)"));

            Sut.Handle(evnt);

            _emailSenderMock.Verify(x => x.Send(It.IsAny<MetricsUpdatedEvent>(), It.IsAny<MetricType>(), It.IsAny<string>()), Times.Once);
        }
    }
}
