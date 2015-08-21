using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Api;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.Common.Utils.Repository;
using ReMi.Contracts.Plugins.Data;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.Auth;
using ReMi.DataEntities.Plugins;
using DataQuery = ReMi.DataEntities.Api.Query;
using DataQueryPermission = ReMi.DataEntities.Auth.QueryPermission;
using DataRole = ReMi.DataEntities.Auth.Role;

namespace ReMi.DataAccess.Tests.Auth
{
    [TestFixture]
    public class QueryPermissionsGatewayTests : TestClassFor<QueryPermissionsGateway>
    {
        private Mock<IRepository<DataQuery>> _queryRepositoryMock;
        private Mock<IRepository<DataQueryPermission>> _queryPermissionRepositoryMock;
        private Mock<IRepository<DataRole>> _roleRepositoryMock;
        private Mock<IRepository<Account>> _accountRepositoryMock;
        private Mock<IRepository<PluginConfiguration>> _pluginConfigurationRepositoryMock;
        private Mock<IMappingEngine> _mapperMock;

        protected override QueryPermissionsGateway ConstructSystemUnderTest()
        {
            return new QueryPermissionsGateway
            {
                QueryRepository = _queryRepositoryMock.Object,
                QueryPermissionRepository = _queryPermissionRepositoryMock.Object,
                RoleRepository = _roleRepositoryMock.Object,
                AccountRepository = _accountRepositoryMock.Object,
                PluginConfigurationRepository = _pluginConfigurationRepositoryMock.Object,
                Mapper = _mapperMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _queryRepositoryMock = new Mock<IRepository<DataQuery>>(MockBehavior.Strict);
            _queryPermissionRepositoryMock = new Mock<IRepository<DataQueryPermission>>(MockBehavior.Strict);
            _roleRepositoryMock = new Mock<IRepository<DataRole>>(MockBehavior.Strict);
            _accountRepositoryMock = new Mock<IRepository<Account>>(MockBehavior.Strict);
            _pluginConfigurationRepositoryMock = new Mock<IRepository<PluginConfiguration>>(MockBehavior.Strict);
            _mapperMock = new Mock<IMappingEngine>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Dispose_ShouldDisposeAllRepositories_WhenInvoked()
        {
            _queryRepositoryMock.Setup(x => x.Dispose());
            _queryPermissionRepositoryMock.Setup(x => x.Dispose());
            _roleRepositoryMock.Setup(x => x.Dispose());
            _accountRepositoryMock.Setup(x => x.Dispose());
            _pluginConfigurationRepositoryMock.Setup(x => x.Dispose());

            Sut.Dispose();

            _queryRepositoryMock.Verify(x => x.Dispose(), Times.Once);
            _queryPermissionRepositoryMock.Verify(x => x.Dispose(), Times.Once);
            _roleRepositoryMock.Verify(x => x.Dispose(), Times.Once);
            _accountRepositoryMock.Verify(x => x.Dispose(), Times.Once);
            _pluginConfigurationRepositoryMock.Verify(x => x.Dispose(), Times.Once);
        }

        [Test]
        public void GetQueries_ShouldReturnAllNotBackgroundQueries_WhenCalledWithoutParameter()
        {
            var queries = BuildQueryEntities();
            var notStaticCount = queries.Count(x => !x.IsStatic);

            _queryRepositoryMock.SetupEntities(queries);
            _mapperMock.Setup(x => x.Map<DataQuery, Query>(It.IsAny<DataQuery>()))
                .Returns(new Query());


            var result = Sut.GetQueries();


            Assert.AreEqual(notStaticCount, result.Count());
            _mapperMock.Verify(x => x.Map<DataQuery, Query>(It.IsAny<DataQuery>()), Times.Exactly(notStaticCount));
        }

        [Test]
        public void GetQueries_ShouldReturnAllQueries_WhenCalledWithoutTrueParameter()
        {
            var queries = BuildQueryEntities();
            var count = queries.Count();

            _queryRepositoryMock.SetupEntities(queries);
            _mapperMock.Setup(x => x.Map<DataQuery, Query>(It.IsAny<DataQuery>()))
                .Returns(new Query());


            var result = Sut.GetQueries(true);


            Assert.AreEqual(count, result.Count());
            _mapperMock.Verify(x => x.Map<DataQuery, Query>(It.IsAny<DataQuery>()), Times.Exactly(count));
        }

        [Test]
        public void AddQueryPermission_ShouldAddNewQueryToRoleRelation_WhenQueryAndRoleExist()
        {
            var roles = BuildRoleEntities();
            var queries = BuildQueryEntities();
            var startPermissionCount = queries.First().QueryPermissions.Count;

            _queryRepositoryMock.SetupEntities(queries);
            _roleRepositoryMock.SetupEntities(roles);
            _queryRepositoryMock.Setup(x => x.Update(queries.First()))
                .Returns((ChangedFields<DataQuery>)null);

            Sut.AddQueryPermission(queries.First().QueryId, roles.First().ExternalId);

            Assert.IsTrue(queries.First().QueryPermissions.Any(x => x.RoleId == roles.First().Id));
            Assert.AreEqual(startPermissionCount + 1, queries.First().QueryPermissions.Count);
            _queryRepositoryMock.Verify(x => x.Update(queries.First()), Times.Once());
        }

        [Test]
        [ExpectedException(typeof(QueryNotFoundException))]
        public void AddQueryPermission_ShouldThrowException_WhenQueryNotExists()
        {
            var roles = BuildRoleEntities();
            var queries = BuildQueryEntities();

            _queryRepositoryMock.SetupEntities(queries);

            Sut.AddQueryPermission(-1, roles.First().ExternalId);
        }

        [Test]
        [ExpectedException(typeof(RoleNotFoundException))]
        public void AddQueryPermission_ShouldThrowException_WhenRoleNotExists()
        {
            var roles = BuildRoleEntities();
            var queries = BuildQueryEntities();

            _queryRepositoryMock.SetupEntities(queries);
            _roleRepositoryMock.SetupEntities(roles);

            Sut.AddQueryPermission(queries.First().QueryId, Guid.Empty);
        }

        [Test]
        public void AddQueryPermission_ShouldDoNothing_WhenPermissionExists()
        {
            var roles = BuildRoleEntities();
            var queries = BuildQueryEntities();
            queries.First().QueryPermissions.Add(new DataQueryPermission
            {
                QueryId = queries.First().QueryId,
                RoleId = roles.First().Id,
                Role = roles.First()
            });
            var startPermissionCount = queries.First().QueryPermissions.Count;

            _queryRepositoryMock.SetupEntities(queries);
            _roleRepositoryMock.SetupEntities(roles);

            Sut.AddQueryPermission(queries.First().QueryId, roles.First().ExternalId);

            Assert.AreEqual(startPermissionCount, queries.First().QueryPermissions.Count);
            _queryRepositoryMock.Verify(x => x.Update(It.IsAny<DataQuery>()), Times.Never);
        }

        [Test]
        public void RemoveQueryPermission_ShouldRemoveQueryToRoleRelation_WhenQueryAndRoleExist()
        {
            var roles = BuildRoleEntities();
            var queries = BuildQueryEntities();
            queries.First().QueryPermissions.Add(new DataQueryPermission
            {
                QueryId = queries.First().QueryId,
                RoleId = roles.First().Id,
                Role = roles.First()
            });

            _queryRepositoryMock.SetupEntities(queries);
            _roleRepositoryMock.SetupEntities(roles);
            _queryPermissionRepositoryMock.SetupEntities(queries.First().QueryPermissions);
            _queryPermissionRepositoryMock.Setup(x =>x.Delete(It.Is<DataQueryPermission>(
                r => r.QueryId == queries.First().QueryId && r.RoleId == roles.First().Id)));

            Sut.RemoveQueryPermission(queries.First().QueryId, roles.First().ExternalId);

            Assert.IsTrue(queries.First().QueryPermissions.Any(x => x.RoleId == roles.First().Id));
            _queryPermissionRepositoryMock.Verify(x => x.Delete(It.Is<DataQueryPermission>(
                r => r.QueryId == queries.First().QueryId && r.RoleId == roles.First().Id)), Times.Once());

        }

        [Test]
        [ExpectedException(typeof(QueryNotFoundException))]
        public void RemoveQueryPermission_ShouldThrowException_WhenQueryNotExists()
        {
            var roles = BuildRoleEntities();
            var queries = BuildQueryEntities();

            _queryRepositoryMock.SetupEntities(queries);

            Sut.RemoveQueryPermission(-1, roles.First().ExternalId);
        }

        [Test]
        [ExpectedException(typeof(RoleNotFoundException))]
        public void RemoveQueryPermission_ShouldThrowException_WhenRoleNotExists()
        {
            var roles = BuildRoleEntities();
            var queries = BuildQueryEntities();

            _queryRepositoryMock.SetupEntities(queries);
            _roleRepositoryMock.SetupEntities(roles);

            Sut.RemoveQueryPermission(queries.First().QueryId, Guid.Empty);
        }

        [Test]
        public void RemoveQueryPermission_ShouldDoNothing_WhenPermissionExists()
        {
            var roles = BuildRoleEntities();
            var queries = BuildQueryEntities();

            _queryRepositoryMock.SetupEntities(queries);
            _roleRepositoryMock.SetupEntities(roles);

            Sut.RemoveQueryPermission(queries.First().QueryId, roles.First().ExternalId);

            _queryPermissionRepositoryMock.Verify(x => x.Delete(It.IsAny<DataQueryPermission>()), Times.Never);
        }

        [Test]
        public void GetAllowedQueries_ShouldReturnCorrectValue()
        {
             var queryPermissions = new List<DataQueryPermission>
             {
                 new DataQueryPermission
                 {
                     Query = new DataQuery{Name = RandomData.RandomString(15,20)},
                     Role = new DataRole
                     {
                         ExternalId = Guid.NewGuid()
                     }
                 }
             };

             _roleRepositoryMock.SetupEntities(queryPermissions.Select(x => x.Role).ToArray());
             _queryPermissionRepositoryMock.SetupEntities(queryPermissions);

            var result = Sut.GetAllowedQueries(queryPermissions[0].Role.ExternalId);

            Assert.AreEqual(queryPermissions[0].Query.Name, result.First());
        }

        [Test]
        public void GetAllowedCommands_ShouldReturnEmptyCollection_WhenRoleIsEmptyAndAuthenticationMethodDefined()
        {
            var queries = Builder<DataQuery>.CreateListOfSize(5).Build();
            var accounts = Builder<Account>.CreateListOfSize(5).Build();
            var pluginsConfig = Builder<PluginConfiguration>.CreateListOfSize(5)
                .Random(1)
                .With(x => x.PluginType, PluginType.Authentication)
                .With(x => x.PluginId, RandomData.RandomInt(int.MaxValue))
                .Build();

            _accountRepositoryMock.SetupEntities(accounts);
            _pluginConfigurationRepositoryMock.SetupEntities(pluginsConfig);
            _queryRepositoryMock.SetupEntities(queries);

            var result = Sut.GetAllowedQueries(null);

            CollectionAssert.IsEmpty(result);
        }

        [Test]
        public void GetAllowedCommands_ShouldReturnAllCommands_WhenRoleIsEmptyAndNoAuthenticationMethodDefined()
        {
            var queries = Builder<DataQuery>.CreateListOfSize(5).Build();

            _accountRepositoryMock.SetupEntities(Enumerable.Empty<Account>());
            _pluginConfigurationRepositoryMock.SetupEntities(Enumerable.Empty<PluginConfiguration>());
            _queryRepositoryMock.SetupEntities(queries);

            var result = Sut.GetAllowedQueries(null);

            CollectionAssert.AreEquivalent(queries.Select(x => x.Name), result);
        }
        
        [Test]
        public void GetAllowedQueries_ShouldReturnAllQueriesWhenRoleIsAdmin()
        {
            var role = new DataRole
            {
                ExternalId = Guid.NewGuid(),
                Name = "Admin"
            };
            var queries = Builder<DataQuery>.CreateListOfSize(5).Build();

            _roleRepositoryMock.SetupEntities(new[] { role });
            _queryRepositoryMock.SetupEntities(queries);

            var result = Sut.GetAllowedQueries(role.ExternalId);

            CollectionAssert.AreEquivalent(queries.Select(x => x.Name), result);
        }

        private static IList<DataQuery> BuildQueryEntities()
        {
            return Builder<DataQuery>.CreateListOfSize(5)
                .All()
                .Do(x => x.QueryId = RandomData.RandomInt(10000))
                .Do(x => x.Description = RandomData.RandomString(10))
                .Do(x => x.Group = RandomData.RandomString(10))
                .Do(x => x.Name = RandomData.RandomString(10))
                .Do(x => x.IsStatic = RandomData.RandomBool())
                .Do(x => x.QueryPermissions = RandomData.RandomBool()
                    ? new List<DataQueryPermission>()
                    : Builder<DataQueryPermission>.CreateListOfSize(RandomData.RandomInt(1, 5))
                    .All()
                    .Do(cp => cp.Role = new DataRole
                    {
                        ExternalId = Guid.NewGuid(),
                        Description = RandomData.RandomString(10),
                        Name = RandomData.RandomString(10),
                        Id = RandomData.RandomInt(10000)
                    })
                    .Build())
                .Build();
        }

        private static IList<DataRole> BuildRoleEntities()
        {
            return Builder<DataRole>.CreateListOfSize(5)
                .All()
                .Do(x => x.Id = RandomData.RandomInt(10000))
                .Do(x => x.Description = RandomData.RandomString(10))
                .Do(x => x.Name = RandomData.RandomString(10))
                .Do(x => x.ExternalId = Guid.NewGuid())
                .Build();
        }
    }
}
