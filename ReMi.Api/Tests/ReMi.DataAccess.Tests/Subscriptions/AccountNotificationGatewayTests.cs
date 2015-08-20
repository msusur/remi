using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Moq;
using NUnit.Framework;
using ReMi.Common.Constants;
using ReMi.Common.Constants.Subscriptions;
using ReMi.Common.Utils.Enums;
using ReMi.Common.Utils.Repository;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.Subscriptions;
using ReMi.DataAccess.Exceptions;
using ReMi.DataAccess.Exceptions.Subscriptions;
using ReMi.DataEntities.Auth;
using ReMi.DataEntities.Products;
using ReMi.DataEntities.Subscriptions;
using Account = ReMi.DataEntities.Auth.Account;

namespace ReMi.DataAccess.Tests.Subscriptions
{
    public class AccountNotificationGatewayTests : TestClassFor<AccountNotificationGateway>
    {
        private Account _account;
        private AccountNotification _accountNotification;

        private Mock<IMappingEngine> _mapperMock;
        private Mock<IRepository<Account>> _accountRepositoryMock;
        private Mock<IRepository<AccountNotification>> _accountNotificationRepositoryMock;
        
        protected override AccountNotificationGateway ConstructSystemUnderTest()
        {
            return new AccountNotificationGateway
            {
                Mapper = _mapperMock.Object,
                AccountRepository = _accountRepositoryMock.Object,
                AccountNotificationRepository = _accountNotificationRepositoryMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _mapperMock = new Mock<IMappingEngine>();
            _accountNotificationRepositoryMock = new Mock<IRepository<AccountNotification>>();
            _accountRepositoryMock = new Mock<IRepository<Account>>();

            var account = new Account
            {
                ExternalId = Guid.NewGuid(),
                AccountId = RandomData.RandomInt(1, 77),
                AccountProducts = new List<AccountProduct>
                {
                    new AccountProduct
                    {
                        Product = new Product
                        {
                            Description = "prod1"
                        }
                    }
                }
            };

            _accountNotification = new AccountNotification
            {
                AccountId = account.AccountId,
                NotificationType = NotificationType.ApiChange,
                Account = account
            };

            _account = account;
            _account.AccountNotifications = new List<AccountNotification> {_accountNotification};

            _accountRepositoryMock.SetupEntities(new []{_account});
            _accountNotificationRepositoryMock.SetupEntities(new[] {_accountNotification});

            base.TestInitialize();
        }

        [Test]
        [ExpectedException(typeof(AccountNotFoundException))]
        public void GetAccountNotifications_ShouldThrowException_WhenAccountNotFound()
        {
            Sut.GetAccountNotifications(Guid.NewGuid());
        }

        [Test]
        public void GetAccountNotifications_ShouldReturnCorrectResult_WhenAccountIsPresent()
        {
            var result = Sut.GetAccountNotifications(_account.ExternalId);

            Assert.AreEqual(8, result.Count());
            Assert.True(
                result.Any(
                    x =>
                        x.Subscribed &&
                        x.NotificationName == EnumDescriptionHelper.GetDescription(NotificationType.ApiChange)));
            Assert.True(
                result.Any(
                    x =>
                        !x.Subscribed &&
                        x.NotificationName ==
                        EnumDescriptionHelper.GetDescription(NotificationType.ReleaseWindowsSchedule)));
            Assert.True(
                result.Any(
                    x =>
                        !x.Subscribed &&
                        x.NotificationName ==
                        EnumDescriptionHelper.GetDescription(NotificationType.Signing)));
            Assert.True(
                result.Any(
                    x =>
                        !x.Subscribed &&
                        x.NotificationName ==
                        EnumDescriptionHelper.GetDescription(NotificationType.Closing)));
            Assert.True(
                result.Any(
                    x =>
                        !x.Subscribed &&
                        x.NotificationName ==
                        EnumDescriptionHelper.GetDescription(NotificationType.Approvement)));
            Assert.True(
                result.Any(
                    x =>
                        !x.Subscribed &&
                        x.NotificationName ==
                        EnumDescriptionHelper.GetDescription(NotificationType.ReleaseTasks)));
        }

        [Test]
        public void GetSubscribers_ShouldCallAutomapper_WhenInvoked()
        {
            Sut.GetSubscribers(NotificationType.ApiChange);

            _mapperMock.Verify(
                x =>
                    x.Map<List<Account>, List<BusinessEntities.Auth.Account>>(
                        It.Is<List<Account>>(l => l.Count == 1 && l[0].AccountId == _account.AccountId)));
        }

        [Test]
        public void GetSubscribers_ShouldCallAutomapper_WhenInvokedWithproductParameter()
        {
            Sut.GetSubscribers(NotificationType.ApiChange, new[] {"prod1"});

            _mapperMock.Verify(
                x =>
                    x.Map<List<Account>, List<BusinessEntities.Auth.Account>>(
                        It.Is<List<Account>>(l => l.Count == 1 && l[0].AccountId == _account.AccountId)));
        }

        [Test]
        [ExpectedException(typeof(AccountNotFoundException))]
        public void RemoveNotificationSubscriptions_ShouldThrowException_WhenAccountNotFound()
        {
            Sut.RemoveNotificationSubscriptions(Guid.NewGuid(), null);
        }

        [Test]
        [ExpectedException(typeof(NotificationSubscriptionNotFoundException))]
        public void RemoveNotificationSubscriptions_ShouldThrowException_WhenSubscriptionNotFound()
        {
            Sut.RemoveNotificationSubscriptions(_account.ExternalId, new[] {NotificationType.ReleaseWindowsSchedule});
        }

        [Test]
        public void RemoveNotificationSubscriptions_ShouldCallRepositoryDelete_WhenSubscriptionExists()
        {
            Sut.RemoveNotificationSubscriptions(_account.ExternalId, new[] {NotificationType.ApiChange});

            _accountNotificationRepositoryMock.Verify(
                x =>
                    x.Delete(
                        It.Is<AccountNotification>(
                            a => a.AccountId == _account.AccountId && a.NotificationType == NotificationType.ApiChange)));
        }

        [Test]
        [ExpectedException(typeof(AccountNotFoundException))]
        public void AddNotificationSubscriptions_ShouldThrowException_WhenAccountNotFound()
        {
            Sut.AddNotificationSubscriptions(Guid.NewGuid(), null);
        }

        [Test]
        [ExpectedException(typeof(DuplicatedNotificationSubscriptionException))]
        public void AddNotificationSubscriptions_ShouldThrowException_WhenSubscriptionAlreadyExists()
        {
            Sut.AddNotificationSubscriptions(_account.ExternalId, new[] { NotificationType.ApiChange });
        }

        [Test]
        public void AddNotificationSubscriptions_ShouldCallRepositoryInsert_WhenSubscriptionDoesNotExist()
        {
            Sut.AddNotificationSubscriptions(_account.ExternalId, new[] {NotificationType.ReleaseWindowsSchedule});

            _accountNotificationRepositoryMock.Verify(
                x =>
                    x.Insert(
                        It.Is<AccountNotification>(
                            a =>
                                a.AccountId == _account.AccountId &&
                                a.NotificationType == NotificationType.ReleaseWindowsSchedule)));
        }

        [Test]
        public void Dispose_ShouldCallDisposeForAllRepos_WhenInvoked()
        {
            Sut.Dispose();

            _accountNotificationRepositoryMock.Verify(x => x.Dispose());
            _accountRepositoryMock.Verify(x => x.Dispose());
        }
    }
}
