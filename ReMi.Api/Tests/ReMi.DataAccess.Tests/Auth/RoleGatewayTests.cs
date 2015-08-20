using System;
using System.Linq.Expressions;
using AutoMapper;
using Moq;
using FizzWare.NBuilder;
using NUnit.Framework;
using ReMi.Common.Utils.Repository;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.AutoMapper;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.BusinessRules;
using BusinessAccount = ReMi.BusinessEntities.Auth.Account;
using DataAccount = ReMi.DataEntities.Auth.Account;

using Role = ReMi.DataEntities.Auth.Role;

namespace ReMi.DataAccess.Tests.Auth
{
    [TestFixture]
    public class RoleGatewayTests : TestClassFor<RoleGateway>
    {
        private Mock<IRepository<DataAccount>> _accountRepositoryMock;
        private Mock<IRepository<Role>> _roleRepositoryMock;
        private Mock<IMappingEngine> _mappingEngineMock;

        protected override RoleGateway ConstructSystemUnderTest()
        {
            _accountRepositoryMock = new Mock<IRepository<DataAccount>>();
            _roleRepositoryMock = new Mock<IRepository<Role>>(MockBehavior.Strict);
            _mappingEngineMock = new Mock<IMappingEngine>();

            return new RoleGateway
            {
                AccountRepository = _accountRepositoryMock.Object,
                RoleRepository = _roleRepositoryMock.Object,
                Mapper = _mappingEngineMock.Object
            };
        }

        [TestFixtureSetUp]
        public static void AutoMapperInitialize()
        {
            Mapper.Initialize(
                c =>
                {
                    c.AddProfile<BusinessEntityToDataEntityMappingProfile>();
                    c.AddProfile(new DataEntityToBusinessEntityMappingProfile());
                });
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateRole_ShouldRaiseException_WhenRoleIsNull()
        {
            _roleRepositoryMock.SetupEntities(new Role[0]);

            Sut.CreateRole(null);
        }

        [Test]
        [ExpectedException(typeof(RoleAlreadyExistsException))]
        public void CreateRole_ShouldRaiseException_WhenExternalIdAlreadyExists()
        {
            var role = Builder<BusinessEntities.Auth.Role>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .Build();

            _roleRepositoryMock.SetupEntities(new[] { new Role { ExternalId = role.ExternalId } });

            Sut.CreateRole(role);
        }

        [Test]
        [ExpectedException(typeof(RoleAlreadyExistsException))]
        public void CreateRole_ShouldRaiseException_WhenNameAlreadyExists()
        {
            var role = Builder<BusinessEntities.Auth.Role>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .Build();

            _roleRepositoryMock.SetupEntities(new[] { new Role { Name = role.Name } });

            Sut.CreateRole(role);
        }

        [Test]
        [ExpectedException(typeof(RoleAlreadyExistsException))]
        public void CreateRole_ShouldRaiseException_WhenDescriptionAlreadyExists()
        {
            var role = Builder<BusinessEntities.Auth.Role>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .Build();

            _roleRepositoryMock.SetupEntities(new[] { new Role { Description = role.Description } });

            Sut.CreateRole(role);
        }

        [Test]
        public void CreateRole_ShouldInsertIntoRepository_WhenInvoked()
        {
            _roleRepositoryMock.SetupEntities(new Role[0]);
            _roleRepositoryMock.Setup(o => o.Insert(It.IsAny<Role>()));

            var role = Builder<BusinessEntities.Auth.Role>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .Build();

            var dataRole = Builder<Role>.CreateNew()
                .With(o => o.ExternalId, role.ExternalId)
                .With(o => o.Name, role.Name)
                .With(o => o.Description, role.Description)
                .Build();


            _mappingEngineMock.Setup(o => o.Map<BusinessEntities.Auth.Role, Role>(role))
                .Returns(dataRole);

            Sut.CreateRole(role);

            _roleRepositoryMock.Verify(
                o => o.Insert(It.Is<Role>(
                    row => row.ExternalId == role.ExternalId
                        && row.Description == role.Description
                        && row.Name == role.Name
                )));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateRole_ShouldRaiseException_WhenRoleIsNull()
        {
            _roleRepositoryMock.SetupEntities(new Role[0]);

            Sut.UpdateRole(null);
        }

        [Test]
        [ExpectedException(typeof(RoleNotFoundException))]
        public void UpdateRole_ShouldRaiseException_WhenExternalIdNotFound()
        {
            var role = Builder<BusinessEntities.Auth.Role>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .Build();

            _roleRepositoryMock.SetupEntities(new Role[0]);

            Sut.UpdateRole(role);
        }

        [Test]
        [ExpectedException(typeof(RoleAlreadyExistsException))]
        public void UpdateRole_ShouldRaiseException_WhenRoleWithTheSimilarNameExists()
        {
            var role = Builder<BusinessEntities.Auth.Role>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.Name, RandomData.RandomString(20))
                .With(o => o.Description, RandomData.RandomString(20))
                .Build();

            var dataRole1 = Builder<Role>.CreateNew()
                .With(o => o.Id, 9)
                .With(o => o.ExternalId, role.ExternalId)
                .With(o => o.Name, RandomData.RandomString(20))
                .With(o => o.Description, RandomData.RandomString(20))
                .Build();

            var dataRole2 = Builder<Role>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.Name, role.Name)
                .With(o => o.Description, RandomData.RandomString(20))
                .Build();

            _roleRepositoryMock.SetupEntities(new[] { dataRole1, dataRole2 });

            Sut.UpdateRole(role);
        }

        [Test]
        [ExpectedException(typeof(RoleAlreadyExistsException))]
        public void UpdateRole_ShouldRaiseException_WhenRoleWithTheSimilarDescriptionExists()
        {
            var role = Builder<BusinessEntities.Auth.Role>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.Name, RandomData.RandomString(20))
                .With(o => o.Description, RandomData.RandomString(20))
                .Build();

            var dataRole1 = Builder<Role>.CreateNew()
                .With(o => o.Id, 9)
                .With(o => o.ExternalId, role.ExternalId)
                .With(o => o.Name, RandomData.RandomString(20))
                .With(o => o.Description, RandomData.RandomString(20))
                .Build();

            var dataRole2 = Builder<Role>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.Name, RandomData.RandomString(20))
                .With(o => o.Description, role.Description)
                .Build();

            _roleRepositoryMock.SetupEntities(new[] { dataRole1, dataRole2 });

            Sut.UpdateRole(role);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Can't update role when attached accounts exists")]
        public void UpdateRole_ShouldRaiseException_WhenAttachedAccountsExists()
        {
            var dataAccount = SetupDataAccount();

            var role = Builder<BusinessEntities.Auth.Role>.CreateNew()
                .With(o => o.ExternalId, dataAccount.Role.ExternalId)
                .With(o => o.Name, dataAccount.Role.Name)
                .With(o => o.Description, dataAccount.Role.Description)
                .Build();

            var dataRole = Builder<Role>.CreateNew()
                .With(o => o.ExternalId, role.ExternalId)
                .With(o => o.Name, role.Name)
                .With(o => o.Description, role.Description)
                .Build();

            _roleRepositoryMock.SetupEntities(new[] { dataRole });

            _accountRepositoryMock.SetupEntities(new[] { dataAccount });

            Sut.UpdateRole(role);
        }

        [Test]
        public void UpdateRole_ShouldUpdateRepository_WhenInvoked()
        {
            var role = Builder<BusinessEntities.Auth.Role>.CreateNew()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.Name, RandomData.RandomString(20))
                .With(o => o.Description, RandomData.RandomString(20))
                .Build();

            var dataRole = Builder<Role>.CreateNew()
                .With(o => o.Id, 9)
                .With(o => o.ExternalId, role.ExternalId)
                .With(o => o.Name, RandomData.RandomString(20))
                .With(o => o.Description, RandomData.RandomString(20))
                .Build();

            _roleRepositoryMock.SetupEntities(new[] { dataRole });

            var checkExternalId = false;
            _roleRepositoryMock.Setup(o => o.Update(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<Action<Role>>()))
                .Returns(new ChangedFields<Role>())
                .Callback<Expression<Func<Role, bool>>, Action<Role>>((exp, act) =>
                {
                    checkExternalId = exp.Compile()(dataRole);
                    act(dataRole);
                });

            _accountRepositoryMock.SetupEntities(new DataAccount[0]);

            Sut.UpdateRole(role);

            _roleRepositoryMock.Verify(o => o.Update(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<Action<Role>>()));

            Assert.IsTrue(checkExternalId);
            Assert.AreEqual(role.ExternalId, dataRole.ExternalId);
            Assert.AreEqual(role.Name, dataRole.Name);
            Assert.AreEqual(role.Description, dataRole.Description);
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

        #endregion
    }
}
