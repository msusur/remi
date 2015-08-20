using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.Common.Constants.BusinessRules;
using ReMi.DataAccess.BusinessEntityGateways.BusinessRules;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.Api;
using ReMi.DataEntities.BusinessRules;
using System;
using System.Linq;
using ReMi.Common.Utils.Repository;
using ReMi.TestUtils.UnitTests;

namespace ReMi.DataAccess.Tests.BusinessRules
{
    [TestFixture]
    public class BusinessRuleGatewayTests : TestClassFor<BusinessRuleGateway>
    {
        private Mock<IRepository<BusinessRuleDescription>> _businessRuleRepositoryMock;
        private Mock<IRepository<BusinessRuleTestData>> _businessRuleTestDataRepositoryMock;
        private Mock<IRepository<BusinessRuleAccountTestData>> _businessRuleAccountTestDataRepositoryMock;
        private Mock<IRepository<Command>> _commandRepositoryMock;
        private Mock<IRepository<Query>> _queryRepositoryMock;
        private Mock<IMappingEngine> _mappingEngineMock;

        protected override BusinessRuleGateway ConstructSystemUnderTest()
        {
            return new BusinessRuleGateway
            {
                BusinessRuleRepository = _businessRuleRepositoryMock.Object,
                BusinessRuleTestDataRepository = _businessRuleTestDataRepositoryMock.Object,
                BusinessRuleAccountTestDataRepository = _businessRuleAccountTestDataRepositoryMock.Object,
                CommandRepository = _commandRepositoryMock.Object,
                QueryRepository = _queryRepositoryMock.Object,
                MappingEngine = _mappingEngineMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _businessRuleRepositoryMock = new Mock<IRepository<BusinessRuleDescription>>(MockBehavior.Strict);
            _businessRuleTestDataRepositoryMock = new Mock<IRepository<BusinessRuleTestData>>(MockBehavior.Strict);
            _businessRuleAccountTestDataRepositoryMock = new Mock<IRepository<BusinessRuleAccountTestData>>(MockBehavior.Strict);
            _commandRepositoryMock = new Mock<IRepository<Command>>(MockBehavior.Strict);
            _queryRepositoryMock = new Mock<IRepository<Query>>(MockBehavior.Strict);
            _mappingEngineMock = new Mock<IMappingEngine>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void DisposeRepositories_ShouldDisposeAllRepositories_WhenGatewayDisposed()
        {
            _businessRuleRepositoryMock.Setup(x => x.Dispose());
            _businessRuleTestDataRepositoryMock.Setup(x => x.Dispose());

            Sut.Dispose();

            _businessRuleRepositoryMock.Verify(x => x.Dispose(), Times.Once);
            _businessRuleTestDataRepositoryMock.Verify(x => x.Dispose(), Times.Once);
        }

        [Test]
        public void GetBusinessRule_ShouldReturnBusinessRuleByExternalId_WhenInvoked()
        {
            var rule = new BusinessRuleDescription { ExternalId = Guid.NewGuid() };
            _businessRuleRepositoryMock.SetupEntities(new[] { rule });

            _mappingEngineMock.Setup(o => o.Map<BusinessRuleDescription, BusinessEntities.BusinessRules.BusinessRuleDescription>(rule))
                .Returns(new BusinessEntities.BusinessRules.BusinessRuleDescription());

            Sut.GetBusinessRule(rule.ExternalId);

            _mappingEngineMock.Verify(o => o.Map<BusinessRuleDescription, BusinessEntities.BusinessRules.BusinessRuleDescription>(rule), Times.Once);
        }

        [Test]
        public void GetBusinessRule_ShouldReturnBusinessRuleByName_WhenInvoked()
        {
            var rule = new BusinessRuleDescription { Name = RandomData.RandomString(10), Group = BusinessRuleGroup.Release };
            _businessRuleRepositoryMock.SetupEntities(new[] { rule });

            _mappingEngineMock.Setup(o => o.Map<BusinessRuleDescription, BusinessEntities.BusinessRules.BusinessRuleDescription>(rule))
                .Returns(new BusinessEntities.BusinessRules.BusinessRuleDescription());

            Sut.GetBusinessRule(BusinessRuleGroup.Release, rule.Name);

            _mappingEngineMock.Verify(o => o.Map<BusinessRuleDescription, BusinessEntities.BusinessRules.BusinessRuleDescription>(rule), Times.Once);
        }

        [Test]
        public void GetBusinessRuleId_ShouldReturnRuleExternalId_WhenInvoked()
        {
            var rule = new BusinessRuleDescription
            {
                Name = RandomData.RandomString(10),
                Group = BusinessRuleGroup.Release,
                ExternalId = Guid.NewGuid()
            };
            _businessRuleRepositoryMock.SetupEntities(new[] { rule });

            var result = Sut.GetBusinessRuleId(BusinessRuleGroup.Release, rule.Name);

            Assert.AreEqual(rule.ExternalId, result);
        }

        [Test]
        public void GetBusinessRules_ShouldReturnAllRules_WhenInvoked()
        {
            var rules = Builder<BusinessRuleDescription>.CreateListOfSize(5)
                .All()
                .Do(x => x.Group = RandomData.RandomEnum<BusinessRuleGroup>())
                .Do(x => x.ExternalId = Guid.NewGuid())
                .Build();

            _businessRuleRepositoryMock.SetupEntities(rules);
            _mappingEngineMock.Setup(o => o.Map<BusinessRuleDescription, BusinessEntities.BusinessRules.BusinessRuleView>(It.IsAny<BusinessRuleDescription>()))
                .Returns((BusinessRuleDescription x) => new BusinessEntities.BusinessRules.BusinessRuleView
                {
                    ExternalId = x.ExternalId,
                    Group = x.Group
                });

            Sut.GetBusinessRules();

            _mappingEngineMock.Verify(o => o.Map<BusinessRuleDescription, BusinessEntities.BusinessRules.BusinessRuleView>(It.IsAny<BusinessRuleDescription>()),
                Times.Exactly(5));
        }

        [Test]
        public void CreateBusinessRule_ShouldCreateNewBusinessRule_WhenInvoked()
        {
            var rule = new BusinessEntities.BusinessRules.BusinessRuleDescription
            {
                Name = RandomData.RandomString(10),
                Group = BusinessRuleGroup.Release,
                ExternalId = Guid.NewGuid()
            };
            var dataRule = new BusinessRuleDescription
            {
                ExternalId = rule.ExternalId
            };
            _mappingEngineMock.Setup(o => o.Map<BusinessEntities.BusinessRules.BusinessRuleDescription, BusinessRuleDescription>(rule))
                .Returns(dataRule);
            _businessRuleRepositoryMock.Setup(x => x.Insert(dataRule));

            Sut.CreateBusinessRule(rule);

            _mappingEngineMock.Verify(o => o.Map<BusinessEntities.BusinessRules.BusinessRuleDescription, BusinessRuleDescription>(rule), Times.Once);
            _businessRuleRepositoryMock.Verify(x => x.Insert(dataRule), Times.Once);
        }

        [Test]
        public void DeleteBusinessRule_ShouldReturnBeforeDoAnything_WhenCannotFindRule()
        {
            _businessRuleRepositoryMock.SetupEntities(new BusinessRuleDescription[0]);

            Sut.DeleteBusinessRule(Guid.NewGuid());

            _businessRuleRepositoryMock.Verify(x => x.Delete(It.IsAny<BusinessRuleDescription>()), Times.Never);
        }

        [Test]
        public void DeleteBusinessRule_ShouldDeleteBusinessRule_WhenNoCommandOrQueryHasThisRuleAssigned()
        {
            var rule = new BusinessRuleDescription
            {
                ExternalId = Guid.NewGuid()
            };
            _businessRuleRepositoryMock.SetupEntities(new[] { rule });
            _commandRepositoryMock.SetupEntities(new Command[0]);
            _queryRepositoryMock.SetupEntities(new Query[0]);
            _businessRuleRepositoryMock.Setup(x => x.Delete(rule));

            Sut.DeleteBusinessRule(rule.ExternalId);

            _businessRuleRepositoryMock.Verify(x => x.Delete(rule), Times.Once);
        }

        [Test]
        public void DeleteBusinessRule_ShouldDeleteBusinessRuleAndClearAssociatedCommand_WhenCommandIsRelatedToThisRule()
        {
            var rule = new BusinessRuleDescription
            {
                ExternalId = Guid.NewGuid(),
                BusinessRuleId = RandomData.RandomInt(int.MaxValue)
            };
            var command = new Command { RuleId = rule.BusinessRuleId };
            _businessRuleRepositoryMock.SetupEntities(new[] { rule });
            _commandRepositoryMock.SetupEntities(new[] { command });
            _queryRepositoryMock.SetupEntities(new Query[0]);
            _businessRuleRepositoryMock.Setup(x => x.Delete(rule));
            _commandRepositoryMock.Setup(x => x.Update(command))
                .Returns((ChangedFields<Command>)null);

            Sut.DeleteBusinessRule(rule.ExternalId);

            _businessRuleRepositoryMock.Verify(x => x.Delete(rule), Times.Once);
            _commandRepositoryMock.Verify(x => x.Update(It.IsAny<Command>()), Times.Once);
            _queryRepositoryMock.Verify(x => x.Update(It.IsAny<Query>()), Times.Never);
        }

        [Test]
        public void DeleteBusinessRule_ShouldDeleteBusinessRuleAndClearAssociatedQuery_WhenQueryIsRelatedToThisRule()
        {
            var rule = new BusinessRuleDescription
            {
                ExternalId = Guid.NewGuid(),
                BusinessRuleId = RandomData.RandomInt(int.MaxValue)
            };
            var query = new Query { RuleId = rule.BusinessRuleId };
            _businessRuleRepositoryMock.SetupEntities(new[] { rule });
            _commandRepositoryMock.SetupEntities(new Command[0]);
            _queryRepositoryMock.SetupEntities(new[] { query });
            _businessRuleRepositoryMock.Setup(x => x.Delete(rule));
            _queryRepositoryMock.Setup(x => x.Update(query))
                .Returns((ChangedFields<Query>) null);

            Sut.DeleteBusinessRule(rule.ExternalId);

            _businessRuleRepositoryMock.Verify(x => x.Delete(rule), Times.Once);
            _commandRepositoryMock.Verify(x => x.Update(It.IsAny<Command>()), Times.Never);
            _queryRepositoryMock.Verify(x => x.Update(It.IsAny<Query>()), Times.Once);
        }

        [Test]
        [ExpectedException(typeof(BusinessRuleNotNotFoundException))]
        public void UpdateRuleScript_ShouldThrowNotFoundException_WhenCannotFindRule()
        {
            _businessRuleRepositoryMock.SetupEntities(new BusinessRuleDescription[0]);

            Sut.UpdateRuleScript(Guid.NewGuid(), string.Empty);

            _businessRuleRepositoryMock.Verify(x => x.Delete(It.IsAny<BusinessRuleDescription>()), Times.Never);
        }

        [Test]
        public void UpdateRuleScript_ShouldUdpateBusinessRuleScript_WhenInvoked()
        {
            var rule = new BusinessRuleDescription
            {
                ExternalId = Guid.NewGuid(),
                Script = RandomData.RandomString(10)
            };
            const string newScript = "new script";

            _businessRuleRepositoryMock.SetupEntities(new[] { rule });
            _businessRuleRepositoryMock.Setup(x => x.Update(It.IsAny<BusinessRuleDescription>()))
                .Returns((ChangedFields<BusinessRuleDescription>) null);

            Sut.UpdateRuleScript(rule.ExternalId, newScript);

            _businessRuleRepositoryMock.Verify(x => x.Update(It.Is<BusinessRuleDescription>(r => r.Script == newScript)), Times.Once);
        }

        [Test]
        [ExpectedException(typeof(BusinessRuleTestDataNotNotFoundException))]
        public void UpdateTestData_ShouldThrowNotFoundException_WhenCannotFindTestData()
        {
            _businessRuleTestDataRepositoryMock.SetupEntities(new BusinessRuleTestData[0]);

            Sut.UpdateTestData(Guid.NewGuid(), string.Empty);

            _businessRuleTestDataRepositoryMock.Verify(x => x.Delete(It.IsAny<BusinessRuleTestData>()), Times.Never);
        }

        [Test]
        public void UpdateTestData_ShouldUdpateTestDataJson_WhenInvoked()
        {
            var testData = new BusinessRuleTestData
            {
                ExternalId = Guid.NewGuid(),
                JsonData = RandomData.RandomString(10)
            };
            const string newJsonData = "new json data";

            _businessRuleTestDataRepositoryMock.SetupEntities(new[] { testData });
            _businessRuleTestDataRepositoryMock.Setup(x => x.Update(It.IsAny<BusinessRuleTestData>()))
                .Returns((ChangedFields<BusinessRuleTestData>)null);

            Sut.UpdateTestData(testData.ExternalId, newJsonData);

            _businessRuleTestDataRepositoryMock.Verify(x => x.Update(It.Is<BusinessRuleTestData>(r => r.JsonData == newJsonData)), Times.Once);
        }

        [Test]
        [ExpectedException(typeof(BusinessRuleTestDataNotNotFoundException))]
        public void UpdateAccountTestData_ShouldThrowNotFoundException_WhenCannotFindTestData()
        {
            _businessRuleAccountTestDataRepositoryMock.SetupEntities(new BusinessRuleAccountTestData[0]);

            Sut.UpdateAccountTestData(Guid.NewGuid(), string.Empty);

            _businessRuleAccountTestDataRepositoryMock.Verify(x => x.Delete(It.IsAny<BusinessRuleAccountTestData>()), Times.Never);
        }

        [Test]
        public void UpdateAccountTestData_ShouldUdpateTestDataJson_WhenInvoked()
        {
            var testData = new BusinessRuleAccountTestData
            {
                ExternalId = Guid.NewGuid(),
                JsonData = RandomData.RandomString(10)
            };
            const string newJsonData = "new json data";

            _businessRuleAccountTestDataRepositoryMock.SetupEntities(new[] { testData });
            _businessRuleAccountTestDataRepositoryMock.Setup(x => x.Update(It.IsAny<BusinessRuleAccountTestData>()))
                .Returns((ChangedFields<BusinessRuleAccountTestData>)null);

            Sut.UpdateAccountTestData(testData.ExternalId, newJsonData);

            _businessRuleAccountTestDataRepositoryMock.Verify(x => x.Update(It.Is<BusinessRuleAccountTestData>(r => r.JsonData == newJsonData)), Times.Once);
        }

        [Test]
        public void AddRuleToCommand_ShouldAssigneRuleToCommand_WhenInvoked()
        {
            var rule = new BusinessRuleDescription
            {
                ExternalId = Guid.NewGuid(),
                BusinessRuleId = RandomData.RandomInt(int.MaxValue)
            };
            var command = new Command
            {
                CommandId = RandomData.RandomInt(int.MaxValue)
            };

            _businessRuleRepositoryMock.SetupEntities(new[] { rule });
            _commandRepositoryMock.Setup(x => x.GetByPrimaryKey(command.CommandId))
                .Returns(command);
            _commandRepositoryMock.Setup(x => x.Update(command))
                .Returns((ChangedFields<Command>)null);

            Sut.AddRuleToCommand(rule.ExternalId, command.CommandId);

            _commandRepositoryMock.Verify(x => x.Update(It.IsAny<Command>()), Times.Once);
        }

        [Test]
        public void AddRuleToQuery_ShouldAssigneRuleToQuery_WhenInvoked()
        {
            var rule = new BusinessRuleDescription
            {
                ExternalId = Guid.NewGuid(),
                BusinessRuleId = RandomData.RandomInt(int.MaxValue)
            };
            var query = new Query
            {
                QueryId = RandomData.RandomInt(int.MaxValue)
            };

            _businessRuleRepositoryMock.SetupEntities(new[] { rule });
            _queryRepositoryMock.Setup(x => x.GetByPrimaryKey(query.QueryId))
                .Returns(query);
            _queryRepositoryMock.Setup(x => x.Update(query))
                .Returns((ChangedFields<Query>)null);

            Sut.AddRuleToQuery(rule.ExternalId, query.QueryId);

            _queryRepositoryMock.Verify(x => x.Update(It.IsAny<Query>()), Times.Once);
        }
    }
}
