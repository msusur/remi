using AutoMapper;
using ReMi.Common.Utils;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.BusinessEntities.Auth;
using ReMi.Common.Utils.Repository;
using ReMi.DataEntities.Products;
using ReMi.DataEntities.ReleaseCalendar;
using Account = ReMi.DataEntities.Auth.Account;
using ReleaseParticipant = ReMi.BusinessEntities.ReleasePlan.ReleaseParticipant;
using Role = ReMi.DataEntities.Auth.Role;
using Session = ReMi.DataEntities.Auth.Session;

namespace ReMi.DataAccess.BusinessEntityGateways.Auth
{
    public class AccountsGateway : BaseGateway, IAccountsGateway
    {
        public IRepository<Account> AccountRepository { get; set; }
        public IRepository<Session> SessionRepository { get; set; }
        public IRepository<AccountProduct> AccountProductRepository { get; set; }
        public IRepository<Role> RoleRepository { get; set; }
        public IRepository<ReleaseWindow> ReleaseWindowRepository { get; set; }
        public IRepository<Product> PackageRepository { get; set; }
        public IMappingEngine Mapper { get; set; }

        public BusinessEntities.Auth.Session StartSession(BusinessEntities.Auth.Account account, Guid sessionId, int sessionDuration)
        {
            if (account == null)
                throw new ArgumentNullException("account");

            var dataAccount = AccountRepository.GetSatisfiedBy(o => o.ExternalId == account.ExternalId);
            if (dataAccount == null)
                throw new AccountNotFoundException(account.ExternalId);

            StopAccountSessions(account.ExternalId);

            if (dataAccount.IsBlocked)
                throw new AccountIsBlockedException(dataAccount.ExternalId);

            var currentTime = SystemTime.Now;

            var dataSession = new Session
            {
                AccountId = dataAccount.AccountId,
                ExternalId = sessionId,
                CreatedOn = currentTime,
                ExpireAfter = currentTime.AddMinutes(sessionDuration)
            };

            Logger.DebugFormat("Starting new session: Account = {0}, Session = {1}", dataAccount, dataSession);

            SessionRepository.Insert(dataSession);

            dataSession.Account = dataAccount;

            return Mapper.Map<Session, BusinessEntities.Auth.Session>(dataSession);
        }

        public BusinessEntities.Auth.Session SignSession(BusinessEntities.Auth.Account account)
        {
            if (account == null)
                throw new ArgumentNullException("account");

            var dataAccount = AccountRepository.GetSatisfiedBy(o => o.ExternalId == account.ExternalId);
            if (dataAccount == null)
                throw new AccountNotFoundException(account.ExternalId);

            var dataSession = new Session
            {
                AccountId = dataAccount.AccountId,
                ExternalId = Guid.NewGuid(),
                CreatedOn = SystemTime.Now,
                Completed = SystemTime.Now,
                Description = "Sign session",
            };

            Logger.DebugFormat("Sign session: Account = {0}, Session = {1}", dataAccount, dataSession);

            SessionRepository.Insert(dataSession);

            dataSession.Account = dataAccount;

            return Mapper.Map<Session, BusinessEntities.Auth.Session>(dataSession);
        }

        public BusinessEntities.Auth.Account GetAccount(Guid externalId, bool forceCheck = false)
        {
            var account = AccountRepository.GetSatisfiedBy(o => o.ExternalId == externalId);
            if (account == null)
            {
                if (forceCheck)
                    throw new AccountNotFoundException(externalId);

                return null;
            }

            return Mapper.Map<Account, BusinessEntities.Auth.Account>(account);
        }

        public BusinessEntities.Auth.Account GetAccountByEmail(string email)
        {
            var account = AccountRepository.GetSatisfiedBy(o => o.Email == email);
            if (account == null)
                return null;

            return Mapper.Map<Account, BusinessEntities.Auth.Account>(account);
        }

        public IEnumerable<BusinessEntities.Auth.Account> GetAccountsByProduct(string product)
        {
            return GetAccounts().Where(o => o.Products.Any(x => x.Name.Equals(product, StringComparison.InvariantCultureIgnoreCase)));
        }

        public IEnumerable<BusinessEntities.Auth.Account> GetAccountsByRole(string roleName)
        {
            return Mapper.Map<IEnumerable<Account>, IEnumerable<BusinessEntities.Auth.Account>>(
                AccountRepository.GetAllSatisfiedBy(x => x.Role.Name == roleName)).ToList();
        }

        public IEnumerable<BusinessEntities.Auth.Account> GetAccounts()
        {
            return Mapper.Map<IEnumerable<Account>, IEnumerable<BusinessEntities.Auth.Account>>(
                AccountRepository.Entities).ToList();
        }

        public IEnumerable<BusinessEntities.Auth.Account> GetAccounts(IEnumerable<Guid> externalIds)
        {
            var ids = externalIds.ToArray();

            return Mapper.Map<IEnumerable<Account>, IEnumerable<BusinessEntities.Auth.Account>>(
                AccountRepository.GetAllSatisfiedBy(x => ids.Contains(x.ExternalId)));
        }

        public void CreateNotExistingReleaseParticipants(IEnumerable<ReleaseParticipant> releaseParticipants)
        {
            var participants = releaseParticipants as IList<ReleaseParticipant> ?? releaseParticipants.ToList();

            CreateNotExistingAccounts(participants.Select(o => o.Account).ToArray());
        }

        public void CreateNotExistingAccounts(IEnumerable<BusinessEntities.Auth.Account> accounts)
        {
            CreateNotExistingAccounts(accounts, "BasicUser");
        }

        public void CreateNotExistingAccounts(IEnumerable<BusinessEntities.Auth.Account> accounts, string roleName)
        {
            var localAccounts = accounts as IList<BusinessEntities.Auth.Account> ?? accounts.ToList();
            var emails = localAccounts.Select(o => o.Email).ToArray();
            var existingAccounts = AccountRepository.GetAllSatisfiedBy(x => emails.Any(o => o == x.Email)).ToList();

            foreach (var account in localAccounts)
                if (existingAccounts.All(o => o.Email != account.Email))
                {
                    CreateNotExistingAccount(account, roleName);
                }
        }

        public void UpdateAccountPackages(Guid accountId, IEnumerable<Guid> packageIds, Guid defaultPackageId)
        {
            if (packageIds.IsNullOrEmpty() || packageIds.All(x => x != defaultPackageId))
                throw new AccountHasNoDefaultProductsException(accountId);

            var existingAccount = AccountRepository.GetSatisfiedBy(o => o.ExternalId == accountId);
            if (existingAccount == null)
                throw new AccountNotFoundException(accountId);

            var existingProducts =
                AccountProductRepository.GetAllSatisfiedBy(o => o.AccountId == existingAccount.AccountId)
                    .ToList();

            for (var i = existingProducts.Count - 1; i >= 0; i--)
            {
                var existingProduct = existingProducts[i];
                var foundId = packageIds.FirstOrDefault(o => o == existingProduct.Product.ExternalId);
                if (foundId == Guid.Empty)
                {
                    AccountProductRepository.Delete(existingProduct.AccountProductId);
                    existingProducts.RemoveAt(i);
                }
                else
                {
                    var isDefault = foundId == defaultPackageId;
                    if (isDefault == existingProduct.IsDefault) continue;

                    var p = existingProduct;
                    AccountProductRepository.Update(o => o.AccountProductId == p.AccountProductId,
                        acp => { acp.IsDefault = isDefault; });
                }
            }

            foreach (var packageExternalId in packageIds.Where(x => existingProducts.All(o => o.Product.ExternalId != x)).Distinct())
            {
                var id = packageExternalId;
                var isDefault = id == defaultPackageId;
                var packageId = PackageRepository.GetSatisfiedBy(x => x.ExternalId == id).ProductId;

                AccountProductRepository.Insert(new AccountProduct
                {
                    AccountId = existingAccount.AccountId,
                    ProductId = packageId,
                    CreatedOn = SystemTime.Now,
                    IsDefault = isDefault
                });
            }
        }

        public bool CreateNotExistingAccount(BusinessEntities.Auth.Account account)
        {
            return CreateNotExistingAccount(account, "BasicUser");
        }

        public bool CreateNotExistingAccount(BusinessEntities.Auth.Account account, string roleName)
        {
            var existingAccount = AccountRepository.GetSatisfiedBy(acc => acc.ExternalId == account.ExternalId);

            if (existingAccount == null)
            {
                AccountRepository.Insert(new Account
                {
                    CreatedOn = SystemTime.Now,
                    Description = "Created automatically",
                    Email = account.Email,
                    ExternalId = account.ExternalId,
                    FullName = account.FullName,
                    Name = account.Name,
                    RoleId = RoleRepository.GetSatisfiedBy(x => x.Name == roleName).Id
                });

                return true;
            }

            return false;
        }

        public BusinessEntities.Auth.Session GetSession(Guid externalId)
        {
            var session = SessionRepository.GetSatisfiedBy(o => o.ExternalId == externalId);
            return session == null ? null : Mapper.Map<Session, BusinessEntities.Auth.Session>(session);
        }

        public IEnumerable<BusinessEntities.Auth.Session> GetSessions(Guid accountId)
        {
            var sessions = AccountRepository.GetSatisfiedBy(a => a.ExternalId == accountId).Sessions;

            return Mapper.Map<ICollection<Session>, ICollection<BusinessEntities.Auth.Session>>(sessions);
        }

        public void StopAccountSessions(Guid accountExternalId)
        {
            var sessions = SessionRepository.GetAllSatisfiedBy(o => o.Account.ExternalId == accountExternalId && o.Completed == null);
            foreach (var session in sessions)
            {
                session.Completed = SystemTime.Now;

                SessionRepository.Update(session);
            }
        }

        public void ProlongSession(Guid sessionId, int sessionDuration)
        {
            var session = SessionRepository.GetSatisfiedBy(x => x.ExternalId == sessionId);

            if (!session.ExpireAfter.HasValue)
            {
                return;
            }

            session.ExpireAfter = SystemTime.Now.AddMinutes(sessionDuration);
            SessionRepository.Update(session);

        }

        public BusinessEntities.Auth.Account CreateAccount(BusinessEntities.Auth.Account account, bool createdFromAdminPage = true)
        {
            if (account == null)
                throw new ArgumentNullException("account");

            var existingAccount = AccountRepository.GetSatisfiedBy(o => o.Email == account.Email || o.ExternalId == account.ExternalId);
            if (existingAccount != null)
                throw new AccountAlreadyExistsException(account.Email);

            var dataAccount = Mapper.Map<BusinessEntities.Auth.Account, Account>(account);
            dataAccount.CreatedOn = SystemTime.Now;
            if (dataAccount.Role != null)
            {
                dataAccount.RoleId = RoleRepository.GetSatisfiedBy(x => x.Name == dataAccount.Role.Name).Id;
                dataAccount.Role = null;
            }
            Logger.DebugFormat("Creating new account: Account = {0}", dataAccount);

            AccountRepository.Insert(dataAccount);

            if (createdFromAdminPage)
            {
                UpdateAccountProducts(account);
            }

            return Mapper.Map<Account, BusinessEntities.Auth.Account>(dataAccount);
        }

        public void UpdateAccount(BusinessEntities.Auth.Account account)
        {
            if (account == null)
                throw new ArgumentNullException("account");

            var existingAccount = AccountRepository.GetSatisfiedBy(o => o.ExternalId == account.ExternalId);
            if (existingAccount == null)
                throw new AccountNotFoundException(account.ExternalId);

            var dataAccount = Mapper.Map<BusinessEntities.Auth.Account, Account>(account);

            AccountRepository.Update(row => row.ExternalId == account.ExternalId, acc =>
            {
                acc.Name = dataAccount.Name;
                acc.FullName = dataAccount.FullName;
                acc.Email = dataAccount.Email;
                acc.IsBlocked = dataAccount.IsBlocked;
                acc.RoleId = RoleRepository.GetSatisfiedBy(x => x.Name == dataAccount.Role.Name).Id;
                acc.Description = dataAccount.Description;
            });

            UpdateAccountProducts(account);
        }

        public void UpdateAccountProducts(BusinessEntities.Auth.Account account)
        {
            if (account == null)
                throw new ArgumentNullException("account");

            if (account.Products.IsNullOrEmpty() || account.Products.Count(o => o.IsDefault) == 0)
                throw new AccountHasNoDefaultProductsException(account.ExternalId);

            if (account.Products.Count(o => o.IsDefault) > 1)
                throw new AccountHasManyDefaultProductsException(account.ExternalId);

            UpdateAccountPackages(account.ExternalId,
                account.Products.Select(x => x.ExternalId),
                account.Products.First(x => x.IsDefault).ExternalId);
        }

        public IEnumerable<BusinessEntities.Auth.Account> GetProductOwners(IEnumerable<string> products)
        {
            var productsLocal = products.ToArray();

            return
                Mapper.Map<IEnumerable<Account>, IEnumerable<BusinessEntities.Auth.Account>>(
                    AccountProductRepository.GetAllSatisfiedBy(
                        ap => ap.Account.Role.Name == "ProductOwner" && productsLocal.Contains(ap.Product.Description))
                        .Select(a => a.Account)
                        .ToList());
        }

        public IEnumerable<BusinessEntities.Auth.Account> GetTeamMembers(String product)
        {
            return
                Mapper.Map<IEnumerable<Account>, IEnumerable<BusinessEntities.Auth.Account>>(
                    AccountProductRepository.GetAllSatisfiedBy(
                        ap => ap.Product.Description == product)
                        .Select(a => a.Account)
                        .ToList());
        }

        public IEnumerable<BusinessEntities.Auth.Account> GetTeamMembersExcludeReleaseSupport(BusinessEntities.ReleaseCalendar.ReleaseWindow releaseWindow)
        {
            var products = releaseWindow.Products.ToArray();

            var result = AccountProductRepository.GetAllSatisfiedBy(
                ap =>
                    products.Contains(ap.Product.Description)
                    && ap.Account.ReleaseParticipants.All(rp => rp.ReleaseWindow.ExternalId != releaseWindow.ExternalId))
                .Select(a => a.Account)
                .ToList();

            return
                Mapper.Map<IEnumerable<Account>, IEnumerable<BusinessEntities.Auth.Account>>(result);
        }

        public void AssociateAccountsWithProduct(IEnumerable<string> accountEmails, Guid releaseWindowId, Func<string, TeamRoleRuleResult> updateRoleFunc)
        {
            var releaseWindow = ReleaseWindowRepository.GetSatisfiedBy(o => o.ExternalId == releaseWindowId);
            if (releaseWindow == null)
                throw new ReleaseWindowNotFoundException(releaseWindowId);

            AssociateAccountsWithProducts(releaseWindow.ReleaseProducts.Select(x => x.Product.ExternalId), accountEmails, updateRoleFunc);
        }

        public void AssociateAccountsWithProducts(IEnumerable<Guid> productIds, IEnumerable<string> accountEmails, Func<string, TeamRoleRuleResult> updateRoleFunc)
        {
            var emails = accountEmails.ToArray();

            var existingAccounts = AccountRepository.GetAllSatisfiedBy(a => emails.Any(x => x == a.Email)).ToList();
            if (existingAccounts.Count != emails.Count())
            {
                throw new AccountNotFoundException(
                    emails.Where(a => existingAccounts.Any(e => e.Email != a)).ToList());
            }

            var productIdsFixed = productIds.ToArray();
            var products = PackageRepository.GetAllSatisfiedBy(x => productIdsFixed.Any(o => o == x.ExternalId)).ToList();

            foreach (var account in existingAccounts)
            {
                var ruleResult = updateRoleFunc == null ? null : updateRoleFunc(account.Role.Name);
                if (ruleResult != null && ruleResult.RequiresUpdate)
                {
                    var id = account.ExternalId;
                    AccountRepository.Update(
                        a => a.ExternalId == id,
                        a =>
                        {
                            a.RoleId = RoleRepository.GetSatisfiedBy(x => x.Name == ruleResult.NewRole).Id;
                        });
                }

                var accountProducts =
                    AccountProductRepository.GetAllSatisfiedBy(a => emails.Any(acc => acc == a.Account.Email)).ToList();

                var existingAccountProducts = accountProducts.Where(ap => ap.Account.ExternalId == account.ExternalId).ToList();

                foreach (var product in products.Where(x => existingAccountProducts.All(a => a.Product.ExternalId != x.ExternalId)))
                {
                    AccountProductRepository.Insert(new AccountProduct
                    {
                        AccountId = account.AccountId,
                        ProductId = product.ProductId,
                        CreatedOn = SystemTime.Now,
                        IsDefault = !existingAccountProducts.Any(o => o.IsDefault)
                    });
                }
            }
        }

        public int GetDataAccountId(Guid externalId)
        {
            return
                AccountRepository.GetSatisfiedBy(a => a.ExternalId == externalId).AccountId;
        }

        public override void OnDisposing()
        {
            AccountRepository.Dispose();
            AccountProductRepository.Dispose();
            RoleRepository.Dispose();
            SessionRepository.Dispose();
            ReleaseWindowRepository.Dispose();

            base.OnDisposing();
        }
    }
}
