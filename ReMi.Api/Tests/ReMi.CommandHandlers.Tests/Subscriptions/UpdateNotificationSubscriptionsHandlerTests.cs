using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Subscriptions;
using ReMi.CommandHandlers.Subscriptions;
using ReMi.Commands.Subscriptions;
using ReMi.Common.Constants.Subscriptions;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.Subscriptions;

namespace ReMi.CommandHandlers.Tests.Subscriptions
{
    public class UpdateNotificationSubscriptionsHandlerTests : TestClassFor<UpdateNotificationSubscriptionsHandler>
    {
        private Mock<IAccountNotificationGateway> _accountNotificationGatewayMock;

        protected override UpdateNotificationSubscriptionsHandler ConstructSystemUnderTest()
        {
            return new UpdateNotificationSubscriptionsHandler
            {
                AccountNotificationGatwayFactory = () => _accountNotificationGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _accountNotificationGatewayMock = new Mock<IAccountNotificationGateway>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallGatewayToRemoveAndAddSubscriptions_WhenCommandRecieved()
        {
            var command = new UpdateNotificationSubscriptionsCommand
            {
                CommandContext = new CommandContext
                {
                    UserId = Guid.NewGuid()
                },
                NotificationSubscriptions = new List<NotificationSubscription>
                {
                    new NotificationSubscription
                    {
                        NotificationName = "Api change",
                        Subscribed = true,
                        NotificationType = NotificationType.ApiChange
                    },
                    new NotificationSubscription
                    {
                        NotificationName = "Release windows schedule",
                        NotificationType = NotificationType.ReleaseWindowsSchedule
                    }
                }
            };

            _accountNotificationGatewayMock.Setup(
                x => x.GetAccountNotifications(command.CommandContext.UserId))
                .Returns(new List<NotificationSubscription>
                {
                    new NotificationSubscription
                    {
                        NotificationName = "Api change",
                        NotificationType = NotificationType.ApiChange
                    },
                    new NotificationSubscription
                    {
                        NotificationName = "Release windows schedule",
                        Subscribed = true,
                        NotificationType = NotificationType.ReleaseWindowsSchedule
                    }
                });

            Sut.Handle(command);

            _accountNotificationGatewayMock.Verify(
                x => x.GetAccountNotifications(command.CommandContext.UserId));
            _accountNotificationGatewayMock.Verify(
                x =>
                    x.AddNotificationSubscriptions(command.CommandContext.UserId,
                        It.Is<List<NotificationType>>(n => n.Count == 1 && n[0] == NotificationType.ApiChange)));
            _accountNotificationGatewayMock.Verify(
                x =>
                    x.RemoveNotificationSubscriptions(command.CommandContext.UserId,
                        It.Is<List<NotificationType>>(
                            n => n.Count == 1 && n[0] == NotificationType.ReleaseWindowsSchedule)));
        }
    }
}
