using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessEntities.Products;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.Queries.Auth;
using ReMi.QueryHandlers.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.TestUtils.UnitTests;

namespace ReMi.QueryHandlers.Tests.Auth
{
    public class GetAccountHandlerTests : TestClassFor<GetAccountHandler>
    {
        private Mock<IProductGateway> _productGatewayMock;
        private Mock<IAccountsGateway> _accountGatewayMock;

        protected override GetAccountHandler ConstructSystemUnderTest()
        {
            return new GetAccountHandler
            {
                AccountsGatewayFactory = () => _accountGatewayMock.Object,
                ProductGatewayFactory = () => _productGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _productGatewayMock = new Mock<IProductGateway>();
            _accountGatewayMock = new Mock<IAccountsGateway>();

            base.TestInitialize();
        }

        [Test]
        public void GetAccount_ShouldReturnNull_WhenInvokedWithNonExistingId()
        {
            var accountId = new Guid();

            var account = Sut.Handle(new GetAccountRequest{AccountId = accountId}).Account;

            Assert.IsNull(account);
        }

        [Test]
        public void GetAccount_ShouldCallGateway_WhenInvoked()
        {
            var accountId = new Guid();

            Sut.Handle(new GetAccountRequest { AccountId = accountId });

            _accountGatewayMock.Verify(o => o.GetAccount(It.Is<Guid>(x => x == accountId), false));
        }

        [Test]
        public void GetAccount_ShouldReturnAccountWithAllProducts_WhenAccountRoleIsAdmin()
        {
            var businessAccount = SetupAccount(null, new Role { Name = "Admin" });

            _productGatewayMock.Setup(x => x.GetAllProducts())
                .Returns(new List<ProductView> { new ProductView() });
            _accountGatewayMock.Setup(x => x.GetAccount(It.IsAny<Guid>(), false))
                .Returns(businessAccount);

            var actual = Sut.Handle(new GetAccountRequest { AccountId = businessAccount.ExternalId }).Account;

            Assert.IsTrue(actual.Products.First().IsDefault);
            _productGatewayMock.Verify(o => o.GetAllProducts(), Times.Once());
        }

        [Test]
        public void GetAccount_ShouldReturnAccountWithAllProductsAndNotFirstProductAsDefult_WhenAccountRoleIsAdminAndProductsAreAssociatedWithAccount()
        {
            var products = CreateProductList();
            var businessAccount = SetupAccount(null, new Role { Name = "Admin" });
            businessAccount.Products = products.Take(3).Select(x => new ProductView { ExternalId = x.ExternalId }).ToList();
            businessAccount.Products.Last().IsDefault = true;

            _productGatewayMock.Setup(x => x.GetAllProducts())
                .Returns(products);
            _accountGatewayMock.Setup(x => x.GetAccount(It.IsAny<Guid>(), false))
                .Returns(businessAccount);

            var actual = Sut.Handle(new GetAccountRequest { AccountId = businessAccount.ExternalId }).Account;

            Assert.IsFalse(actual.Products.First().IsDefault);
            Assert.IsTrue(actual.Products.Any(x => x.IsDefault));
            _productGatewayMock.Verify(o => o.GetAllProducts(), Times.Once());
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(MoreThanOneDefaultProductException))]
        public void GetAccount_ShouldThrowException_WhenAccountContainsMoreThanOneDefaultProduct()
        {
            var businessAccount = SetupAccount(null, new Role { Name = "BasicUser" });
            businessAccount.Products = CreateProductList();
            businessAccount.Products.First().IsDefault = true;
            businessAccount.Products.Last().IsDefault = true;

            _accountGatewayMock.Setup(x => x.GetAccount(It.IsAny<Guid>(), false))
                .Returns(businessAccount);

            var actual = Sut.Handle(new GetAccountRequest { AccountId = businessAccount.ExternalId }).Account;
        }

        private Account SetupAccount(string email = null)
        {
            return SetupAccount(email, new Role { Name = "NotAuthenticated" });
        }
        private Account SetupAccount(string email, Role role)
        {
            var account = Builder<Account>.CreateNew()
                .With(o => o.Role, role.Name != "NotAuthenticated" ? role : new Role { Name = new[] { "Admin", "BasicUser", "NotAuthenticated", "ProductOwner" }[RandomData.RandomInt(0, 3)] })
                .With(o => o.Email, email ?? RandomData.RandomEmail())
                .Build();

            _accountGatewayMock.Setup(o => o.GetAccountByEmail(account.Email))
                .Returns(account);

            return account;
        }

        private IEnumerable<ProductView> CreateProductList()
        {
            return Builder<ProductView>.CreateListOfSize(5)
                .All()
                .With(x => x.IsDefault, false)
                .Do(x => x.ExternalId = Guid.NewGuid())
                .Do(x => x.Name = RandomData.RandomString(5))
                .Build();
        }
        }
}
