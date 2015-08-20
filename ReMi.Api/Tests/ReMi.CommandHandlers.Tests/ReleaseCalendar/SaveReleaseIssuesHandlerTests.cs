using System;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.CommandHandlers.ReleaseExecution;
using ReMi.Commands.ReleaseCalendar;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.Events.ReleaseCalendar;

namespace ReMi.CommandHandlers.Tests.ReleaseCalendar
{
    public class SaveReleaseIssuesHandlerTests : TestClassFor<SaveReleaseIssuesHandler>
    {
        private Mock<IReleaseWindowGateway> _releaseWindowGatewayMock;
        private Mock<IPublishEvent> _eventPublisherMock;

        protected override SaveReleaseIssuesHandler ConstructSystemUnderTest()
        {
            return new SaveReleaseIssuesHandler
            {
                ReleaseWindowGatewayFactory = () => _releaseWindowGatewayMock.Object,
                EventPublisher = _eventPublisherMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _eventPublisherMock = new Mock<IPublishEvent>();
           _releaseWindowGatewayMock = new Mock<IReleaseWindowGateway>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_SaveReleaseIssues()
        {
            var saveReleaseIssuesCommand = new SaveReleaseIssuesCommand
            {
                ReleaseWindow = new ReleaseWindow
                {
                    Issues = RandomData.RandomString(50, 100),
                    ExternalId = Guid.NewGuid()
                }
            };

            Sut.Handle(saveReleaseIssuesCommand);

            _releaseWindowGatewayMock.Verify(m => m.SaveIssues(saveReleaseIssuesCommand.ReleaseWindow));
            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<ReleaseIssuesUpdatedEvent>(
                            x =>
                                x.ReleaseWindowId == saveReleaseIssuesCommand.ReleaseWindow.ExternalId &&
                                x.Issues == saveReleaseIssuesCommand.ReleaseWindow.Issues)));
        }
    }
}
