using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.Subscriptions;
using ReMi.Common.Constants;
using ReMi.Common.Constants.Subscriptions;
using ReMi.Common.Utils.Enums;
using ReMi.Common.Utils.Repository;
using ReMi.DataAccess.Exceptions;
using ReMi.DataAccess.Exceptions.Subscriptions;
using ReMi.DataEntities.Subscriptions;

namespace ReMi.DataAccess.BusinessEntityGateways.Subscriptions
{
    public class AccountNotificationGateway : BaseGateway, IAccountNotificationGateway
    {
        public IRepository<DataEntities.Auth.Account> AccountRepository { get; set; }
        public IRepository<AccountNotification> AccountNotificationRepository { get; set; }
        public IMappingEngine Mapper { get; set; }

        public IEnumerable<NotificationSubscription> GetAccountNotifications(Guid accountId)
        {
            var account = AccountRepository.GetSatisfiedBy(x => x.ExternalId == accountId);

            if (account == null)
            {
                throw new AccountNotFoundException(accountId);
            }

            var notificaions =
                EnumDescriptionHelper.GetEnumDescriptions<NotificationType, NotificationTypeDescription>()
                    .Select(x => new NotificationSubscription
                    {
                        NotificationName = x.Description,
                        Subscribed =
                            account.AccountNotifications.Any(
                                a =>
                                    a.NotificationType ==
                                    (NotificationType) Enum.Parse(typeof (NotificationType), x.Name)),
                        NotificationType = (NotificationType) Enum.Parse(typeof (NotificationType), x.Name)
                    }).ToList();

            return notificaions;
        }

        public IEnumerable<Account> GetSubscribers(NotificationType notificationType)
        {
            var accounts = 
                AccountNotificationRepository.GetAllSatisfiedBy(x => x.NotificationType == notificationType)
                    .Select(a => a.Account)
                    .ToList();

            return Mapper.Map<List<DataEntities.Auth.Account>, List<Account>>(accounts);
        }

        public IEnumerable<Account> GetSubscribers(NotificationType notificationType, IEnumerable<string> products)
        {
            var productsLocal = products.ToList();
            
            var accounts =
                AccountNotificationRepository.GetAllSatisfiedBy(
                    x =>
                        x.NotificationType == notificationType &&
                        x.Account.AccountProducts.Any(p => productsLocal.Contains(p.Product.Description)))
                    .Select(a => a.Account).ToList();

            return Mapper.Map<List<DataEntities.Auth.Account>, List<Account>>(accounts);
        }

        public void RemoveNotificationSubscriptions(Guid accountId, IEnumerable<NotificationType> notificationTypes)
        {
            var account = AccountRepository.GetSatisfiedBy(x => x.ExternalId == accountId);

            if (account == null)
            {
                throw new AccountNotFoundException(accountId);
            }

            var subscriptions =
                AccountNotificationRepository.GetAllSatisfiedBy(
                    x => notificationTypes.Contains(x.NotificationType) && x.AccountId == account.AccountId).ToList();

            var missingNotifications =
                notificationTypes.Where(x => subscriptions.All(s => s.NotificationType != x)).ToList();

            if (missingNotifications.Any())
            {
                throw new NotificationSubscriptionNotFoundException(missingNotifications, accountId);
            }

            foreach (var subscription in subscriptions)
            {
                AccountNotificationRepository.Delete(subscription);
            }
        }

        public void AddNotificationSubscriptions(Guid accountId, IEnumerable<NotificationType> notificationTypes)
        {
            var account = AccountRepository.GetSatisfiedBy(x => x.ExternalId == accountId);

            if (account == null)
            {
                throw new AccountNotFoundException(accountId);
            }

            var subscriptions =
                AccountNotificationRepository.GetAllSatisfiedBy(
                    x => notificationTypes.Contains(x.NotificationType) && x.AccountId == account.AccountId).ToList();

            var notificationTypesLocal = notificationTypes as IList<NotificationType> ?? notificationTypes.ToList();

            var existingNotifications =
                notificationTypesLocal.Where(x => subscriptions.Any(s => s.NotificationType == x)).ToList();
            
            if (existingNotifications.Any())
            {
                throw new DuplicatedNotificationSubscriptionException(existingNotifications, accountId);
            }

            foreach (var notificationType in notificationTypesLocal)
            {
                AccountNotificationRepository.Insert(new AccountNotification
                {
                    AccountId = account.AccountId,
                    NotificationType = notificationType
                });
            }
        }

        public override void OnDisposing()
        {
            AccountRepository.Dispose();
            AccountNotificationRepository.Dispose();

            base.OnDisposing();
        }
    }
}
