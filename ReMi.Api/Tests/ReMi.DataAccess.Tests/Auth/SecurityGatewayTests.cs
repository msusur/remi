using System.Linq;
using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.Common.Utils.Repository;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataEntities.Api;
using ReMi.DataEntities.Auth;

namespace ReMi.DataAccess.Tests.Auth
{
    [TestFixture]
    public class SecurityGatewayTests : TestClassFor<SecurityGateway>
    {
        private Mock<IRepository<Command>> _commandRepositoryMock;
        private Mock<IRepository<Query>> _queryRepositoryMock;

        private Mock<IMappingEngine> _mapperMock;

        protected override SecurityGateway ConstructSystemUnderTest()
        {
            _commandRepositoryMock = new Mock<IRepository<Command>>();
            _queryRepositoryMock = new Mock<IRepository<Query>>();
            _mapperMock = new Mock<IMappingEngine>();

            return new SecurityGateway
            {
                CommandRepository = _commandRepositoryMock.Object,
                QueryRepository = _queryRepositoryMock.Object,
                Mapper = _mapperMock.Object
            };
        }

        [Test]
        public void GetCommandRoles_ShouldReturnPermittedRoles_WhenInvoked()
        {
            var commands = Builder<Command>.CreateListOfSize(5)
                .All()
                .Do(x => x.Name = RandomData.RandomString(10))
                .Do(x => x.Description = RandomData.RandomString(10))
                .Do(x => x.CommandId = RandomData.RandomInt(99999999))
                .Do(x => x.CommandPermissions = Builder<CommandPermission>.CreateListOfSize(5)
                    .All()
                    .Do(p => p.Role = new Role { Name = RandomData.RandomString(5) })
                    .Build())
                .Build();

            _commandRepositoryMock.SetupEntities(commands);
            _mapperMock.Setup(x => x.Map<Role, BusinessEntities.Auth.Role>(It.IsAny<Role>()))
                .Returns((Role r) => new BusinessEntities.Auth.Role { Name = r.Name });

            var result = Sut.GetCommandRoles(commands[2].Name);

            Assert.AreEqual(5, result.Count(), "There should be 5 roles in result");
            _mapperMock.Verify(x => x.Map<Role, BusinessEntities.Auth.Role>(It.IsAny<Role>()), Times.Exactly(5));
        }

        [Test]
        public void GetQueryRoles_ShouldReturnPermittedRoles_WhenInvoked()
        {
            var queries = Builder<Query>.CreateListOfSize(5)
                .All()
                .Do(x => x.Name = RandomData.RandomString(10))
                .Do(x => x.Description = RandomData.RandomString(10))
                .Do(x => x.QueryId = RandomData.RandomInt(99999999))
                .Do(x => x.QueryPermissions = Builder<QueryPermission>.CreateListOfSize(5)
                    .All()
                    .Do(p => p.Role = new Role { Name = RandomData.RandomString(5) })
                    .Build())
                .Build();

            _queryRepositoryMock.SetupEntities(queries);
            _mapperMock.Setup(x => x.Map<Role, BusinessEntities.Auth.Role>(It.IsAny<Role>()))
                .Returns((Role r) => new BusinessEntities.Auth.Role { Name = r.Name });

            var result = Sut.GetQueryRoles(queries[2].Name);

            Assert.AreEqual(5, result.Count(), "There should be 5 roles in result");
            _mapperMock.Verify(x => x.Map<Role, BusinessEntities.Auth.Role>(It.IsAny<Role>()), Times.Exactly(5));
        }
    }
}
