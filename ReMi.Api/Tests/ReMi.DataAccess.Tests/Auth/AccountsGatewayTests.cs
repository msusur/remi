using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using Moq;
using FizzWare.NBuilder;
using NUnit.Framework;
using ReMi.Common.Utils;
using ReMi.DataAccess.AutoMapper;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities;
using ReMi.DataEntities.Auth;
using ReMi.DataEntities.ReleaseCalendar;
using BusinessAccount = ReMi.BusinessEntities.Auth.Account;
using DataAccount = ReMi.DataEntities.Auth.Account;
using DataProduct = ReMi.DataEntities.Products.Product;
using BusinessSession = ReMi.BusinessEntities.Auth.Session;
using DataSession = ReMi.DataEntities.Auth.Session;
using ReMi.BusinessEntities.Products;
using ReMi.Common.Utils.Repository;
using ReMi.TestUtils.UnitTests;
using Role = ReMi.DataEntities.Auth.Role;

namespace ReMi.DataAccess.Tests.Auth
{
    [TestFixture]
    public class AccountsGatewayTests : TestClassFor<AccountsGateway>
    {
        private Mock<IRepository<DataAccount>> _accountRepositoryMock;
        private Mock<IRepository<DataSession>> _sessionRepositoryMock;
        private Mock<IRepository<AccountProduct>> _accountProductRepositoryMock;
        private Mock<IRepository<Role>> _roleRepositoryMock;
        private Mock<IMappingEngine> _mappingEngineMock;
        private Mock<IRepository<DataProduct>> _packageRepositoryMock;
        private Mock<IRepository<ReleaseWindow>> _releaseWindowRepositoryMock;

        protected override AccountsGateway ConstructSystemUnderTest()
        {
            _accountRepositoryMock = new Mock<IRepository<DataAccount>>();
            _sessionRepositoryMock = new Mock<IRepository<DataSession>>();
            _accountProductRepositoryMock = new Mock<IRepository<AccountProduct>>();
            _roleRepositoryMock = new Mock<IRepository<Role>>(MockBehavior.Strict);
            _mappingEngineMock = new Mock<IMappingEngine>();
            _packageRepositoryMock = new Mock<IRepository<DataProduct>>();
            _releaseWindowRepositoryMock = new Mock<IRepository<ReleaseWindow>>();

            return new AccountsGateway
            {
                AccountRepository = _accountRepositoryMock.Object,
                AccountProductRepository = _accountProductRepositoryMock.Object,
                RoleRepository = _roleRepositoryMock.Object,
                SessionRepository = _sessionRepositoryMock.Object,
                Mapper = _mappingEngineMock.Object,
                PackageRepository = _packageRepositoryMock.Object,
                ReleaseWindowRepository = _releaseWindowRepositoryMock.Object
            };
        }

        [TestFixtureSetUp]
        public static void AutoMapperInitialize()
        {
            Mapper.Initialize(
                c =>
                {
                    c.AddProfile<BusinessEntityToDataEntityMappingProfile>();
                    c.AddProfile<DataEntityToBusinessEntityMappingProfile>();
                });
        }

        [Test]
        public void GetAccount_ShouldReturnNull_WhenInvokedWithNonExistingAccountId()
        {
            var accountId = Guid.NewGuid();

            var account = Sut.GetAccount(accountId);

            Assert.IsNull(account);
        }

        [Test]
        [ExpectedException(typeof(AccountNotFoundException))]
        public void GetAccount_ShouldRaiseExceptionForNonExistingAccountId_WhenCheckForExastanceForced()
        {
            var accountId = Guid.NewGuid();

            Sut.GetAccount(accountId, true);
        }

        [Test]
        public void GetAccount_ShouldCallMapper_WhenInvoked()
        {
            var dataAccount = SetupDataAccount();

            Sut.GetAccount(dataAccount.ExternalId);

            _mappingEngineMock.Verify(o => o.Map<DataAccount, BusinessAccount>(
                It.Is<DataAccount>(x =>
                    x.CreatedOn == dataAccount.CreatedOn
                    && x.Description == dataAccount.Description
                    && x.Email == dataAccount.Email
                    && x.ExternalId == dataAccount.ExternalId
                    && x.FullName == dataAccount.FullName
                    && x.IsBlocked == dataAccount.IsBlocked
                    && x.Name == dataAccount.Name)));
        }

        [Test]
        public void GetAccountByEmail_ShouldReturnNull_WhenEmailNotFound()
        {
            var dataAccount = SetupDataAccount();

            var result = Sut.GetAccountByEmail(dataAccount.Email);

            Assert.IsNull(result);
        }

        [Test]
        public void GetAccountByEmail_ShouldCallMapper_WhenEmailFound()
        {
            var dataAccount = SetupDataAccount();

            Sut.GetAccountByEmail(dataAccount.Email);

            _mappingEngineMock.Verify(o => o.Map<DataAccount, BusinessAccount>(
                It.Is<DataAccount>(x =>
                    x.CreatedOn == dataAccount.CreatedOn
                    && x.Description == dataAccount.Description
                    && x.Email == dataAccount.Email
                    && x.ExternalId == dataAccount.ExternalId
                    && x.FullName == dataAccount.FullName
                    && x.IsBlocked == dataAccount.IsBlocked
                    && x.Name == dataAccount.Name)));
        }

        [Test]
        public void GetAccountsByRole_ShouldCallMapper_WhenAccountWithRoleFound()
        {
            var dataAccount = SetupDataAccount();

            Sut.GetAccountsByRole(dataAccount.Role.Name);

            _mappingEngineMock.Verify(o => o.Map<IEnumerable<DataAccount>, IEnumerable<BusinessAccount>>(
                It.Is<IEnumerable<DataAccount>>(x => x.Count() == 1
                    && x.First().CreatedOn == dataAccount.CreatedOn
                    && x.First().Description == dataAccount.Description
                    && x.First().Email == dataAccount.Email
                    && x.First().ExternalId == dataAccount.ExternalId
                    && x.First().FullName == dataAccount.FullName
                    && x.First().IsBlocked == dataAccount.IsBlocked
                    && x.First().Name == dataAccount.Name)));
        }

        [Test]
        public void GetAccounts_ShouldCallMapper_WhenAccountsWithRequiredExternalIdsFound()
        {
            var dataAccount1 = Builder<DataAccount>.CreateNew().With(o => o.ExternalId, Guid.NewGuid()).Build();
            var dataAccount2 = Builder<DataAccount>.CreateNew().With(o => o.ExternalId, Guid.NewGuid()).Build();
            var dataAccount3 = Builder<DataAccount>.CreateNew().With(o => o.ExternalId, Guid.NewGuid()).Build();

            _accountRepositoryMock.SetupEntities(new[] { dataAccount1, dataAccount2, dataAccount3 });

            Sut.GetAccounts(new[] { dataAccount1.ExternalId, dataAccount3.ExternalId });

            _mappingEngineMock.Verify(o => o.Map<IEnumerable<DataAccount>, IEnumerable<BusinessAccount>>(
                It.Is<IEnumerable<DataAccount>>(x => x.Count() == 2
                    && x.Any(a => a.ExternalId == dataAccount1.ExternalId)
                    && x.All(a => a.ExternalId != dataAccount2.ExternalId)
                    && x.Any(a => a.ExternalId == dataAccount3.ExternalId))));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterAccount_ShouldRaiseArgumentNullException_WhenInvokedWithNull()
        {
            Sut.CreateAccount(null);
        }

        [Test]
        [ExpectedException(typeof(AccountAlreadyExistsException))]
        public void CreateAccount_ShouldRaiseAlreadyExistsException_WhenInvokedWithExistingAccountId()
        {
            var accountId = Guid.NewGuid();

            SetupDataAccount(accountId);

            var bAcc = CreateAccount(accountId);

            Sut.CreateAccount(bAcc);
        }

        [Test]
        public void CreateAccount_ShouldCallMapper_WhenInvoked()
        {
            var accountId = Guid.NewGuid();

            var bAcc = CreateAccount(accountId);

            var dataAccount = Builder<DataAccount>.CreateNew()
                .With(o => o.ExternalId, accountId)
                .Build();

            _mappingEngineMock.Setup(o => o.Map<BusinessAccount, DataAccount>(bAcc))
                .Returns(dataAccount);

            _accountRepositoryMock.Setup(o => o.Insert(It.IsAny<DataAccount>()))
                .Callback(() => _accountRepositoryMock.SetupEntities(new[] { dataAccount }));

            _packageRepositoryMock.SetupEntities(new[]
            {
                new DataProduct
                {
                    Description = bAcc.Products.ToList()[0].Name,
                    ExternalId = bAcc.Products.ToList()[0].ExternalId
                }
            });

            _roleRepositoryMock.SetupEntities(new[] { new Role { Name = bAcc.Role.Name } });

            Sut.CreateAccount(bAcc);

            _mappingEngineMock.Verify(o => o.Map<BusinessAccount, DataAccount>(
                It.Is<BusinessAccount>(x =>
                    x.IsBlocked == bAcc.IsBlocked
                    && x.Description == bAcc.Description
                    && x.ExternalId == bAcc.ExternalId
                    && x.Email == bAcc.Email
                    && x.FullName == bAcc.FullName
                    && x.Name == bAcc.Name
                    && x.CreatedOn == bAcc.CreatedOn
                    && x.Role.Equals(bAcc.Role)
                )));
        }

        [Test]
        public void CreateAccount_ShouldInsertAccount_WhenInvoked()
        {
            var accountId = Guid.NewGuid();

            var bAcc = CreateAccount(accountId);

            var dataAccount = Builder<DataAccount>.CreateNew()
                .With(o => o.ExternalId, accountId)
                .Build();

            _mappingEngineMock.Setup(o => o.Map<BusinessAccount, DataAccount>(bAcc)).Returns(dataAccount);

            _accountRepositoryMock.Setup(o => o.Insert(It.IsAny<DataAccount>()))
                .Callback(() => _accountRepositoryMock.SetupEntities(new[] { dataAccount }));

            _packageRepositoryMock.SetupEntities(new[]
            {
                new DataProduct
                {
                    Description = bAcc.Products.ToList()[0].Name,
                    ExternalId = bAcc.Products.ToList()[0].ExternalId
                }
            });

            Sut.CreateAccount(bAcc);

            _accountRepositoryMock.Verify(o => o.Insert(It.Is<DataAccount>(x =>
                    x.AccountId == dataAccount.AccountId
                    && x.ExternalId == dataAccount.ExternalId
                    && x.Email == dataAccount.Email
                    && x.Name == dataAccount.Name
                    && x.FullName == dataAccount.FullName
                    && x.Role == dataAccount.Role
                )));
        }

        [Test]
        public void CreateAccount_ShouldMapAccountBeforeReturn_WhenInvoked()
        {
            var accountId = Guid.NewGuid();

            var bAcc = CreateAccount(accountId);

            var dataAccount = Builder<DataAccount>.CreateNew()
                .With(o => o.ExternalId, accountId)
                .Build();

            _mappingEngineMock.Setup(o => o.Map<BusinessAccount, DataAccount>(bAcc))
                .Returns(dataAccount);

            _accountRepositoryMock.Setup(o => o.Insert(It.IsAny<DataAccount>()))
                .Callback(() => _accountRepositoryMock.SetupEntities(new[] { dataAccount }));

            _packageRepositoryMock.SetupEntities(new[]
            {
                new DataProduct
                {
                    Description = bAcc.Products.ToList()[0].Name,
                    ExternalId = bAcc.Products.ToList()[0].ExternalId
                }
            });

            Sut.CreateAccount(bAcc);

            _mappingEngineMock.Verify(o => o.Map<DataAccount, BusinessAccount>(
                It.Is<DataAccount>(x =>
                    x.AccountId == dataAccount.AccountId
                    && x.ExternalId == dataAccount.ExternalId
                    && x.Name == dataAccount.Name
                    && x.FullName == dataAccount.FullName
                    && x.Email == dataAccount.Email
                    && x.Role == dataAccount.Role
                )));
        }

        [Test]
        public void GetSession_ShouldReturnNull_WhenInvokedWithNonExistingSessionId()
        {
            var sessionId = Guid.NewGuid();

            var session = Sut.GetSession(sessionId);

            Assert.IsNull(session);
        }

        [Test]
        public void GetSession_ShouldCallMapper_WhenInvoked()
        {
            var accountId = RandomData.RandomInt(1, int.MaxValue);

            var dataSession = SetupDataSession(accountId);

            Sut.GetSession(dataSession.ExternalId);

            _mappingEngineMock.Verify(o => o.Map<DataSession, BusinessSession>(
                It.Is<DataSession>(x =>
                    x.CreatedOn == dataSession.CreatedOn
                    && x.Description == dataSession.Description
                    && x.ExternalId == dataSession.ExternalId
                    && x.AccountId == dataSession.AccountId
                    && x.Completed == dataSession.Completed
                    && x.ExpireAfter == dataSession.ExpireAfter
                )));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StartSession_ShouldRaiseArgumentNullException_WhenInvokedWithNull()
        {
            Sut.StartSession(null, Guid.NewGuid(), 15);
        }

        [Test]
        [ExpectedException(typeof(AccountNotFoundException))]
        public void StartSession_ShouldRaiseAccountNotFoundException_WhenRequestedAccountnotFound()
        {
            var accountId = Guid.NewGuid();

            var businessAccount = CreateAccount(accountId);

            Sut.StartSession(businessAccount, Guid.NewGuid(), 15);
        }

        [Test]
        public void StartSession_ShouldStopExistingSessions_WhenInvoked()
        {
            var accountId = Guid.NewGuid();

            var businessAccount = CreateAccount(accountId);

            var dataAccount = Builder<DataAccount>.CreateNew()
                            .With(o => o.ExternalId, accountId)
                            .Build();

            _accountRepositoryMock.SetupEntities(new[] { dataAccount });

            SetupDataSession(dataAccount, Guid.NewGuid());

            Sut.StartSession(businessAccount, Guid.NewGuid(), 15);

            _sessionRepositoryMock.Verify(o => o.Update(It.Is<DataSession>(x => x.Completed.HasValue)));
        }

        [Test]
        public void StartSession_ShouldInsertSession_WhenInvoked()
        {
            var accountId = Guid.NewGuid();

            var businessAccount = CreateAccount(accountId);

            var businessSession = Builder<BusinessSession>.CreateNew().Build();

            var dataAccount = Builder<DataAccount>.CreateNew()
                .With(o => o.ExternalId, accountId)
                .Build();
            var sessionId = Guid.NewGuid();

            _accountRepositoryMock.SetupEntities(new[] { dataAccount });

            _mappingEngineMock.Setup(o => o.Map<DataSession, BusinessSession>(It.IsAny<DataSession>()))
                .Returns(businessSession);

            var session = Sut.StartSession(businessAccount, sessionId, 15);

            _sessionRepositoryMock.Verify(o =>
                o.Insert(It.Is<DataSession>(
                    x => x.AccountId == dataAccount.AccountId
                         && x.ExternalId == sessionId
                         && x.ExpireAfter.HasValue
                         && x.ExpireAfter.Value.AddMinutes(-15).Equals(x.CreatedOn)
                         && x.Completed == null
                    )));
        }

        [Test]
        public void StartSession_ShouldInvokeMappers_WhenInvoked()
        {
            var accountId = Guid.NewGuid();

            var businessAccount = CreateAccount(accountId);

            var dataAccount = Builder<DataAccount>.CreateNew()
                .With(o => o.ExternalId, accountId)
                .Build();

            _accountRepositoryMock.SetupEntities(new[] { dataAccount });

            Sut.StartSession(businessAccount, Guid.NewGuid(), 15);

            _mappingEngineMock.Verify(o => o.Map<DataSession, BusinessSession>(It.IsAny<DataSession>()));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SignSession_ShouldRaiseArgumentNullException_WhenInvokedWithNull()
        {
            Sut.SignSession(null);
        }

        [Test]
        [ExpectedException(typeof(AccountNotFoundException))]
        public void SignSession_ShouldRaiseAccountNotFoundException_WhenRequestedAccountNotFound()
        {
            var accountId = Guid.NewGuid();

            var businessAccount = CreateAccount(accountId);

            Sut.SignSession(businessAccount);
        }

        [Test]
        public void SignSession_ShouldInsertCompletedSession_WhenInvoked()
        {
            var accountId = Guid.NewGuid();

            var businessAccount = CreateAccount(accountId);

            var businessSession = Builder<BusinessSession>.CreateNew().Build();

            var dataAccount = Builder<DataAccount>.CreateNew()
                .With(o => o.ExternalId, accountId)
                .Build();

            _accountRepositoryMock.SetupEntities(new[] { dataAccount });

            _mappingEngineMock.Setup(o => o.Map<DataSession, BusinessSession>(It.IsAny<DataSession>()))
                .Returns(businessSession);

            Sut.SignSession(businessAccount);

            _sessionRepositoryMock.Verify(o =>
                o.Insert(It.Is<DataSession>(
                x => x.AccountId == dataAccount.AccountId
                    && x.ExternalId != Guid.Empty
                    && x.ExpireAfter == null
                    && x.Completed != null
                    && x.Description == "Sign session"
                )));
        }

        [Test]
        [ExpectedException(typeof(AccountHasNoDefaultProductsException))]
        public void UpdateAccountProducts_ShouldRaiseException_WhenAccountProductsEmpty()
        {
            var accountId = Guid.NewGuid();

            var bAcc = CreateAccount(accountId);
            bAcc.Products = new ProductView[0];

            var dataAccount = SetupDataAccount(bAcc.ExternalId);
            dataAccount.Sessions = new List<DataSession> { new DataSession() };
            _accountRepositoryMock.SetupEntities(new[] { dataAccount });

            _mappingEngineMock.Setup(o => o.Map<BusinessAccount, DataAccount>(bAcc)).Returns(dataAccount);

            Sut.UpdateAccountProducts(bAcc);
        }

        [Test]
        [ExpectedException(typeof(AccountHasManyDefaultProductsException))]
        public void UpdateAccountProducts_ShouldRaiseException_WhenAccountProductsContainsSeveralDefaultProducts()
        {
            var accountId = Guid.NewGuid();

            var bAcc = CreateAccount(accountId);
            bAcc.Products = new[] { new ProductView { IsDefault = true }, new ProductView { IsDefault = true } };

            var dataAccount = SetupDataAccount(bAcc.ExternalId);
            dataAccount.Sessions = new List<DataSession> { new DataSession() };
            _accountRepositoryMock.SetupEntities(new[] { dataAccount });

            _mappingEngineMock.Setup(o => o.Map<BusinessAccount, DataAccount>(bAcc)).Returns(dataAccount);

            Sut.UpdateAccountProducts(bAcc);
        }

        [Test]
        public void UpdateAccountProducts_ShouldInsertAccountProducts_WhenNoAccountProductsInDatabase()
        {
            var accountId = Guid.NewGuid();

            var bAcc = CreateAccount(accountId);
            bAcc.Products = new[]
            {
                Builder<ProductView>.CreateNew().With(o => o.IsDefault = true).With(o => o.ExternalId, Guid.NewGuid()).Build(), 
                Builder<ProductView>.CreateNew().With(o => o.IsDefault = false).With(o => o.ExternalId, Guid.NewGuid()).Build(), 
                Builder<ProductView>.CreateNew().With(o => o.IsDefault = false).With(o => o.ExternalId, Guid.NewGuid()).Build()
            };

            var products = new[]
            {
                new DataProduct
                {
                    ProductId = RandomData.RandomInt(45, 54),
                    Description = bAcc.Products.ToList()[0].Name,
                    ExternalId = bAcc.Products.ToList()[0].ExternalId
                },
                new DataProduct
                {
                    ProductId = RandomData.RandomInt(145, 154),
                    Description = bAcc.Products.ToList()[1].Name,
                    ExternalId = bAcc.Products.ToList()[1].ExternalId
                },
                new DataProduct
                {
                    ProductId = RandomData.RandomInt(65, 74),
                    Description = bAcc.Products.ToList()[2].Name,
                    ExternalId = bAcc.Products.ToList()[2].ExternalId
                }
            };

            _packageRepositoryMock.SetupEntities(products);

            var dataAccount = SetupDataAccount(bAcc.ExternalId);
            dataAccount.Sessions = new List<DataSession> { new DataSession() };
            _accountRepositoryMock.SetupEntities(new[] { dataAccount });

            _mappingEngineMock.Setup(o => o.Map<BusinessAccount, DataAccount>(bAcc)).Returns(dataAccount);

            Sut.UpdateAccountProducts(bAcc);

            _accountProductRepositoryMock.Verify(o => o.Insert(It.Is<AccountProduct>(
                x => x.AccountId == dataAccount.AccountId && x.ProductId == products[0].ProductId && x.IsDefault)), Times.Once);
            _accountProductRepositoryMock.Verify(o => o.Insert(It.Is<AccountProduct>(
                x => x.AccountId == dataAccount.AccountId && x.ProductId == products[1].ProductId && x.IsDefault == false)), Times.Once);
            _accountProductRepositoryMock.Verify(o => o.Insert(It.Is<AccountProduct>(
                x => x.AccountId == dataAccount.AccountId && x.ProductId == products[2].ProductId && x.IsDefault == false)), Times.Once);
        }

        [Test]
        public void UpdateAccountProducts_ShouldInsertAccountProducts_WhenSomeAccountProductsNotInDatabase()
        {
            var accountId = Guid.NewGuid();

            var bAcc = CreateAccount(accountId);
            bAcc.Products = new[]
            {
                Builder<ProductView>.CreateNew().With(o => o.IsDefault = true).With(o => o.ExternalId, Guid.NewGuid()).Build(), 
                Builder<ProductView>.CreateNew().With(o => o.IsDefault = false).With(o => o.ExternalId, Guid.NewGuid()).Build(), 
                Builder<ProductView>.CreateNew().With(o => o.IsDefault = false).With(o => o.ExternalId, Guid.NewGuid()).Build()
            };

            var products = new[]
            {
                new DataProduct
                {
                    ProductId = RandomData.RandomInt(45, 54),
                    Description = bAcc.Products.ToList()[0].Name,
                    ExternalId = bAcc.Products.ToList()[0].ExternalId
                },
                new DataProduct
                {
                    ProductId = RandomData.RandomInt(145, 154),
                    Description = bAcc.Products.ToList()[1].Name,
                    ExternalId = bAcc.Products.ToList()[1].ExternalId
                },
                new DataProduct
                {
                    ProductId = RandomData.RandomInt(65, 74),
                    Description = bAcc.Products.ToList()[2].Name,
                    ExternalId = bAcc.Products.ToList()[2].ExternalId
                },
                new DataProduct
                {
                    ProductId = RandomData.RandomInt(155, 200),
                    Description = RandomData.RandomString(10),
                    ExternalId = Guid.NewGuid()
                }
            };

            _packageRepositoryMock.SetupEntities(products);

            var dataAccount = SetupDataAccount(bAcc.ExternalId);
            dataAccount.Sessions = new List<DataSession> { new DataSession() };
            _accountRepositoryMock.SetupEntities(new[] { dataAccount });

            _mappingEngineMock.Setup(o => o.Map<BusinessAccount, DataAccount>(bAcc)).Returns(dataAccount);
            var accountProducts = new[]
            {
                Builder<AccountProduct>.CreateNew()
                    .With(o => o.IsDefault = false)
                    .With(o => o.AccountProductId, 1)
                    .With(o => o.AccountId, dataAccount.AccountId)
                    .With(o => o.ProductId, products[0].ProductId)
                    .With(o => o.Product, products[0]).Build(),
                Builder<AccountProduct>.CreateNew()
                    .With(o => o.IsDefault = false)
                    .With(o => o.AccountProductId, 2)
                    .With(o => o.AccountId, dataAccount.AccountId)
                    .With(o => o.ProductId, products[3].ProductId)
                    .With(o => o.Product, products[3]).Build()

            };
            _accountProductRepositoryMock.SetupEntities(accountProducts);
            _accountProductRepositoryMock.Setup(x => x.GetByPrimaryKey(2))
                .Returns(accountProducts[1]);
            _accountProductRepositoryMock.Setup(x => x.Delete(accountProducts[1]))
                .Callback((AccountProduct a) =>
                {
                    a.Product = null;
                });

            Sut.UpdateAccountProducts(bAcc);

            _accountProductRepositoryMock.Verify(o => o.Insert(It.Is<AccountProduct>(
               x => x.AccountId == dataAccount.AccountId && x.ProductId == products[0].ProductId && x.IsDefault)), Times.Never);
            _accountProductRepositoryMock.Verify(o => o.Insert(It.Is<AccountProduct>(
                x => x.AccountId == dataAccount.AccountId && x.ProductId == products[1].ProductId && x.IsDefault == false)), Times.Once);
            _accountProductRepositoryMock.Verify(o => o.Insert(It.Is<AccountProduct>(
                x => x.AccountId == dataAccount.AccountId && x.ProductId == products[2].ProductId && x.IsDefault == false)), Times.Once);
        }

        [Test]
        public void UpdateAccountPackages_ShouldInsertAccountPackages_WhenNoAccountPackageInDatabase()
        {
            var accountId = Guid.NewGuid();
            var packageIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var defaultPackageId = packageIds.ElementAt(1);

            var products = new[]
            {
                new DataProduct
                {
                    ProductId = RandomData.RandomInt(45, 54),
                    Description = RandomData.RandomString(10),
                    ExternalId = packageIds.ElementAt(0)
                },
                new DataProduct
                {
                    ProductId = RandomData.RandomInt(145, 154),
                    Description = RandomData.RandomString(10),
                    ExternalId = packageIds.ElementAt(1)
                },
                new DataProduct
                {
                    ProductId = RandomData.RandomInt(65, 74),
                    Description = RandomData.RandomString(10),
                    ExternalId = packageIds.ElementAt(2)
                }
            };

            _packageRepositoryMock.SetupEntities(products);

            var dataAccount = SetupDataAccount(accountId);
            _accountRepositoryMock.SetupEntities(new[] { dataAccount });

            Sut.UpdateAccountPackages(accountId, packageIds, defaultPackageId);

            _accountProductRepositoryMock.Verify(o => o.Insert(It.Is<AccountProduct>(
                x => x.AccountId == dataAccount.AccountId && x.ProductId == products[0].ProductId && x.IsDefault == false)), Times.Once);
            _accountProductRepositoryMock.Verify(o => o.Insert(It.Is<AccountProduct>(
                x => x.AccountId == dataAccount.AccountId && x.ProductId == products[1].ProductId && x.IsDefault)), Times.Once);
            _accountProductRepositoryMock.Verify(o => o.Insert(It.Is<AccountProduct>(
                x => x.AccountId == dataAccount.AccountId && x.ProductId == products[2].ProductId && x.IsDefault == false)), Times.Once);
        }

        [Test]
        [ExpectedException(typeof(AccountNotFoundException))]
        public void UpdateAccountPackages_ShouldThrowException_WhenAccountDoesNotExist()
        {
            var accountId = Guid.NewGuid();
            var packageIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var defaultPackageId = packageIds.ElementAt(1);


            var dataAccount = SetupDataAccount(Guid.NewGuid());
            _accountRepositoryMock.SetupEntities(new[] { dataAccount });

            Sut.UpdateAccountPackages(accountId, packageIds, defaultPackageId);
        }

        [Test]
        [ExpectedException(typeof(AccountHasNoDefaultProductsException))]
        public void UpdateAccountPackages_ShouldThrowException_WhenDefaultPackageIdIsNotOnTheList()
        {
            var accountId = Guid.NewGuid();
            var packageIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var defaultPackageId = Guid.NewGuid();

            Sut.UpdateAccountPackages(accountId, packageIds, defaultPackageId);
        }

        [Test]
        public void GetTeamMembersExcludeReleaseSupport_ShouldReturnCorrectAccounts()
        {
            var product = RandomData.RandomString(5);

            var window = new ReleaseWindow
            {
                ExternalId = Guid.NewGuid(),
                ReleaseProducts = new[] { new ReleaseProduct { Product = new DataEntities.Products.Product { Description = product } } }
            };
            var businessWindow = new BusinessEntities.ReleaseCalendar.ReleaseWindow
            {
                ExternalId = window.ExternalId,
                Products = new[] { product }
            };
            var account = new DataAccount
            {
                ExternalId = Guid.NewGuid(),
                ReleaseParticipants =
                    new List<ReleaseParticipant>
                    {
                        new ReleaseParticipant {ReleaseWindow = new ReleaseWindow {ExternalId = Guid.NewGuid()}}
                    }
            };
            var otherAccount = new DataAccount
            {
                ExternalId = Guid.NewGuid(),
                ReleaseParticipants =
                    new List<ReleaseParticipant>
                    {
                        new ReleaseParticipant {ReleaseWindow = window}
                    }
            };
            var oneMoreAccount = new DataAccount
            {
                ExternalId = Guid.NewGuid(),
                ReleaseParticipants =
                    new List<ReleaseParticipant>
                    {
                        new ReleaseParticipant {ReleaseWindow = new ReleaseWindow {ExternalId = Guid.NewGuid()}}
                    }
            };
            _accountProductRepositoryMock.SetupEntities(new List<AccountProduct>
            {
                new AccountProduct
                {
                    Account = account,
                    Product = new DataEntities.Products.Product {Description = product}
                },
                 new AccountProduct
                {
                    Account = otherAccount,
                    Product = new DataEntities.Products.Product {Description =product}
                },
                 new AccountProduct
                {
                    Account = oneMoreAccount,
                    Product = new DataEntities.Products.Product {Description = RandomData.RandomString(12)}
                },
            });

            Sut.GetTeamMembersExcludeReleaseSupport(businessWindow);

            _mappingEngineMock.Verify(
                m =>
                    m.Map<IEnumerable<DataAccount>, IEnumerable<BusinessAccount>>(
                        It.Is<IEnumerable<DataAccount>>(x => x.All(a => a.ExternalId == account.ExternalId))));

        }

        [Test]
        public void GetDataAccountId_ShouldReturnCorrectData()
        {
            var externalId = Guid.NewGuid();
            var acc = new DataAccount
            {
                ExternalId = externalId,
                Email = RandomData.RandomEmail(),
                AccountId = RandomData.RandomInt(100, 400)
            };
            _accountRepositoryMock.SetupEntities(new List<DataAccount>
            {
                new DataAccount {ExternalId = Guid.NewGuid(), Email = RandomData.RandomEmail()},
                acc
            });

            var result = Sut.GetDataAccountId(externalId);

            Assert.AreEqual(acc.AccountId, result, "account id");
        }

        [Test]
        [ExpectedException(typeof(ReleaseWindowNotFoundException))]
        public void AssociateAccountsWithProduct_ShouldThrowException_WhenReleaseWindowNotFound()
        {
            Sut.AssociateAccountsWithProduct(null, Guid.NewGuid());
        }

        [Test]
        [ExpectedException(typeof(AccountNotFoundException))]
        public void AssociateAccountsWithProduct_ShouldThrowException_WhenAccountNotFound()
        {
            var product = RandomData.RandomString(5);

            var windowId = Guid.NewGuid();

            var window = new ReleaseWindow
            {
                ExternalId = windowId,
                ReleaseProducts = new[] { new ReleaseProduct { Product = new DataEntities.Products.Product { Description = product } } }
            };

            var accounts = new List<BusinessEntities.Auth.Account>
            {
                new BusinessEntities.Auth.Account {Email = RandomData.RandomString(21, 34)}
            };

            _accountRepositoryMock.SetupEntities(new List<DataAccount>());
            _releaseWindowRepositoryMock.SetupEntities(new[] { window });

            Sut.AssociateAccountsWithProduct(accounts.Select(x => x.Email), windowId);
        }

        [Test]
        public void AssociateAccountsWithProduct_ShouldTWorkCorrectly()
        {
            var product = RandomData.RandomString(5);
            var windowId = Guid.NewGuid();

            var window = new ReleaseWindow
            {
                ExternalId = windowId,
                ReleaseProducts = new[] { new ReleaseProduct { Product = new DataEntities.Products.Product { Description = product } } }
            };

            var accounts = new List<BusinessEntities.Auth.Account>
            {
                new BusinessEntities.Auth.Account
                {
                    Email = RandomData.RandomEmail()
                },
                new BusinessEntities.Auth.Account
                {
                    Email = RandomData.RandomEmail()
                }
            };

            var role = new Role { Id = RandomData.RandomInt(12, 32), Name = "TeamMember" };

            var dataAccounts = new List<DataAccount>
            {
                new DataAccount
                {
                    AccountId = RandomData.RandomInt(12, 34),
                    Email = accounts[0].Email,
                    Role = new Role {Name = RandomData.RandomString(12, 34)},
                    ExternalId = Guid.NewGuid()
                },
                new DataAccount
                {
                    AccountId = RandomData.RandomInt(35, 54),
                    Email = accounts[1].Email,
                    Role = new Role {Name = "BasicUser"},
                    ExternalId = Guid.NewGuid()
                }
            };

            _packageRepositoryMock.SetupEntities(new[] { new DataProduct { Description = product } });
            _accountRepositoryMock.SetupEntities(dataAccounts);
            _releaseWindowRepositoryMock.SetupEntities(new[] { window });
            _roleRepositoryMock.SetupEntities(new[] { role });

            Sut.AssociateAccountsWithProduct(accounts.Select(x => x.Email), windowId);

            _accountProductRepositoryMock.Verify(
                x => x.Insert(It.Is<AccountProduct>(e => e.AccountId == dataAccounts[0].AccountId)));
            _accountProductRepositoryMock.Verify(
                x => x.Insert(It.Is<AccountProduct>(e => e.AccountId == dataAccounts[1].AccountId)));
            _accountRepositoryMock.Verify(
                x => x.Update(It.IsAny<Expression<Func<DataAccount, bool>>>(), It.IsAny<Action<DataAccount>>()),
                Times.Once);
        }

        [Test]
        public void GetSessions_ShouldCallMapperWithCorrectParameters()
        {
            var accountId = Guid.NewGuid();

            var bAcc = CreateAccount(accountId);
            var dataAccount = SetupDataAccount(bAcc.ExternalId);
            dataAccount.Sessions = new List<DataSession> { new DataSession() };
            _accountRepositoryMock.SetupEntities(new[] { dataAccount });

            Sut.GetSessions(accountId);

            _mappingEngineMock.Verify(
                m => m.Map<ICollection<DataSession>, ICollection<BusinessEntities.Auth.Session>>(dataAccount.Sessions));
        }

        [Test]
        public void ProlongSession_ShouldMoveExpirationDateIn15Minutes()
        {
            SystemTime.Mock(DateTime.UtcNow);

            var session = new DataSession
            {
                ExternalId = Guid.NewGuid(),
                ExpireAfter = SystemTime.Now.AddMinutes(2)
            };
            _sessionRepositoryMock.SetupEntities(new[] { session });

            Sut.ProlongSession(session.ExternalId, 15);

            _sessionRepositoryMock.Verify(
                x =>
                    x.Update(
                        It.Is<DataSession>(
                            s =>
                                s.ExternalId == session.ExternalId && s.ExpireAfter.HasValue &&
                                s.ExpireAfter.Value.Equals(SystemTime.Now.AddMinutes(15)))));
        }

        [Test]
        public void ProlongSession_ShouldNotProlongSessionWithoutExpirationDate()
        {
            SystemTime.Mock(DateTime.UtcNow);

            var session = new DataSession
            {
                ExternalId = Guid.NewGuid()
            };
            _sessionRepositoryMock.SetupEntities(new[] { session });

            Sut.ProlongSession(session.ExternalId, 15);

            _sessionRepositoryMock.Verify(
                x =>
                    x.Update(
                        It.Is<DataSession>(
                            s =>
                                s.ExternalId == session.ExternalId && s.ExpireAfter.HasValue &&
                                s.ExpireAfter.Value.Equals(SystemTime.Now.AddMinutes(15)))), Times.Never);
        }

        #region Helpers

        private DataAccount SetupDataAccount()
        {
            return SetupDataAccount(Guid.NewGuid());
        }

        private DataAccount SetupDataAccount(Guid accountId)
        {
            var dataAccount = Builder<DataAccount>.CreateNew()
                    .With(o => o.ExternalId, accountId)
                    .With(o => o.AccountId, RandomData.RandomInt(1, int.MaxValue))
                    .With(o => o.Role, new Role
                    {
                        ExternalId = Guid.NewGuid(),
                        Name = RandomData.RandomString(30),
                        Description = RandomData.RandomString(50),
                    })
                    .Build();

            _accountRepositoryMock.SetupEntities(new[] { dataAccount });

            return dataAccount;
        }

        private DataSession SetupDataSession(int accountId)
        {
            return SetupDataSession(accountId, Guid.NewGuid());
        }

        private DataSession SetupDataSession(int accountId, Guid sessionId)
        {
            return SetupDataSession(
                Builder<DataAccount>.CreateNew()
                    .With(o => o.AccountId, accountId)
                    .Build(),
                sessionId);
        }

        private DataSession SetupDataSession(DataAccount account, Guid sessionId)
        {
            var dataSession = Builder<DataSession>.CreateNew()
                    .With(o => o.ExternalId, sessionId)
                    .With(o => o.AccountId, account.AccountId)
                    .With(o => o.SessionId, RandomData.RandomInt(1, int.MaxValue))
                    .With(o => o.Account, account)
                    .With(o => o.ExpireAfter, null)
                    .With(o => o.Completed, null)
                    .Build();

            _sessionRepositoryMock.SetupEntities(new[] { dataSession });

            return dataSession;
        }

        private BusinessEntities.Auth.Role CreateRole(Guid? externalId = null)
        {
            return new BusinessEntities.Auth.Role
            {
                ExternalId = externalId ?? Guid.NewGuid(),
                Name = RandomData.RandomString(30),
                Description = RandomData.RandomString(50),
            };
        }

        private BusinessAccount CreateAccount(Guid? accountId = null)
        {
            var acc = Builder<BusinessAccount>.CreateNew()
                        .With(o => o.ExternalId, accountId ?? Guid.NewGuid())
                        .With(o => o.Products, new[] {
                            new ProductView { ExternalId = Guid.NewGuid(), Name = RandomData.RandomString(5), IsDefault = true}
                        })
                        .With(o => o.Role, CreateRole())
                        .Build();
            return acc;
        }

        #endregion
    }
}
