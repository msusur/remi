using System;
using Moq;
using NUnit.Framework;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.Subscriptions;
using ReMi.Queries.Subscriptions;
using ReMi.QueryHandlers.Subscriptions;

namespace ReMi.QueryHandlers.Tests.Subscriptions
{
    public class NotificationSubscriptionHandlerTests : TestClassFor<NotificationSubscriptionHandler>
    {
        private Mock<IAccountNotificationGateway> _accountNotificationsGatewayMock;

        protected override NotificationSubscriptionHandler ConstructSystemUnderTest()
        {
            return new NotificationSubscriptionHandler
            {
                AccountNotificationGatewayFactory = () => _accountNotificationsGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _accountNotificationsGatewayMock = new Mock<IAccountNotificationGateway>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallAccountNotificationGateway_WhenIsInvoked()
        {
            var request = new GetAccountNotificationSubscriptionsRequest
            {
                AccountId = Guid.NewGuid()
            };

            Sut.Handle(request);

            _accountNotificationsGatewayMock.Verify(x => x.GetAccountNotifications(request.AccountId));
        }
    }
}
