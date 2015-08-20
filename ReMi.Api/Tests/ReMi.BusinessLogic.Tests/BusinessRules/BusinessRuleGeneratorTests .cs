using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessLogic.Api;
using ReMi.BusinessLogic.BusinessRules;
using ReMi.Common.Constants.BusinessRules;
using ReMi.Common.Utils;
using ReMi.Common.Utils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.Auth;

namespace ReMi.BusinessLogic.Tests.BusinessRules
{
    [TestFixture]
    public class BusinessRuleGeneratorTests : TestClassFor<BusinessRuleGenerator>
    {
        private Mock<IApiDescriptionBuilder> _apiDescriptionBuilderMock;
        private Mock<IAccountsGateway> _accountGatewayMock;
        private Mock<ISerialization> _serializationMock;
        private readonly Account _account = new Account { FullName = "Test Account", ExternalId = Guid.NewGuid() };

        protected override void TestInitialize()
        {
            _apiDescriptionBuilderMock = new Mock<IApiDescriptionBuilder>(MockBehavior.Strict);
            _serializationMock = new Mock<ISerialization>(MockBehavior.Strict);
            _accountGatewayMock = new Mock<IAccountsGateway>(MockBehavior.Strict);
            _accountGatewayMock.Setup(x => x.Dispose());

            base.TestInitialize();
        }

        protected override BusinessRuleGenerator ConstructSystemUnderTest()
        {
            return new BusinessRuleGenerator
            {
                ApiDescriptionBuilder = _apiDescriptionBuilderMock.Object,
                Serialization = _serializationMock.Object,
                AccountGatewayFactory = () => _accountGatewayMock.Object
            };
        }

        [Test]
        public void GenerateCommandRule_ShouldGenerateCommandPermissionRule_WhenCommandTypeIsGiven()
        {
            _serializationMock.Setup(x => x.ToJson(_account, null, It.IsAny<bool>()))
                .Returns("account json");
            _apiDescriptionBuilderMock.Setup(x => x.FormatType(typeof(TestCommand), 0, null))
                .Returns("command json");
            _accountGatewayMock.Setup(x => x.GetAccount(_account.ExternalId, true))
                .Returns(_account);

            var result = Sut.GenerateCommandRule(typeof(TestCommand), _account.ExternalId);

            Assert.AreEqual("Rule for command TestCommand", result.Description);
            Assert.AreEqual("TestCommandRule", result.Name);
            Assert.AreNotEqual(Guid.Empty, result.ExternalId);
            Assert.AreEqual(BusinessRuleGroup.Permissions, result.Group);
            Assert.IsEmpty(result.Script);
            Assert.AreNotEqual(Guid.Empty, result.AccountTestData.ExternalId);
            Assert.AreEqual("account json", result.AccountTestData.JsonData);
            Assert.AreEqual(1, result.Parameters.Count());
            Assert.AreNotEqual(Guid.Empty, result.Parameters.First().ExternalId);
            Assert.AreEqual("command", result.Parameters.First().Name);
            Assert.AreEqual("ReMi.BusinessLogic.Tests.BusinessRules.BusinessRuleGeneratorTests+TestCommand", result.Parameters.First().Type);
            Assert.AreNotEqual(Guid.Empty, result.Parameters.First().TestData.ExternalId);
            Assert.AreEqual("command json", result.Parameters.First().TestData.JsonData);
        }

        [Test]
        public void GenerateQueryRule_ShouldGenerateQueryPermissionRule_WhenQueryTypeIsGiven()
        {
            _serializationMock.Setup(x => x.ToJson(_account, null, It.IsAny<bool>()))
                .Returns("account json");
            _apiDescriptionBuilderMock.Setup(x => x.FormatType(typeof(TestQuery), 0, null))
                .Returns("query json");
            _accountGatewayMock.Setup(x => x.GetAccount(_account.ExternalId, true))
                .Returns(_account);

            var result = Sut.GenerateQueryRule(typeof(TestQuery), _account.ExternalId);

            Assert.AreEqual("Rule for query TestQuery", result.Description);
            Assert.AreEqual("TestQueryRule", result.Name);
            Assert.AreNotEqual(Guid.Empty, result.ExternalId);
            Assert.AreEqual(BusinessRuleGroup.Permissions, result.Group);
            Assert.IsEmpty(result.Script);
            Assert.AreNotEqual(Guid.Empty, result.AccountTestData.ExternalId);
            Assert.AreEqual("account json", result.AccountTestData.JsonData);
            Assert.AreEqual(1, result.Parameters.Count());
            Assert.AreNotEqual(Guid.Empty, result.Parameters.First().ExternalId);
            Assert.AreEqual("query", result.Parameters.First().Name);
            Assert.AreEqual("ReMi.BusinessLogic.Tests.BusinessRules.BusinessRuleGeneratorTests+TestQuery", result.Parameters.First().Type);
            Assert.AreNotEqual(Guid.Empty, result.Parameters.First().TestData.ExternalId);
            Assert.AreEqual("query json", result.Parameters.First().TestData.JsonData);
        }

        private class TestCommand : ICommand
        {
            public CommandContext CommandContext { get; set; }
        }
        private class TestQuery : IQuery
        {
            public QueryContext Context { get; set; }
        }
    }
}
