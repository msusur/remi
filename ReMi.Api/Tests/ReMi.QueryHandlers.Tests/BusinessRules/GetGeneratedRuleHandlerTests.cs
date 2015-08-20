using System;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.BusinessRules;
using ReMi.BusinessLogic.BusinessRules;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Queries.BusinessRules;
using ReMi.QueryHandlers.BusinessRules;

namespace ReMi.QueryHandlers.Tests.BusinessRules
{
    public class GetGeneratedRuleHandlerTests : TestClassFor<GetGeneratedRuleHandler>
    {
        private Mock<IBusinessRuleEngine> _businessRuleEngineMock;
        private Mock<IBusinessRuleGenerator> _businessRuleGeneratorMock;

        protected override GetGeneratedRuleHandler ConstructSystemUnderTest()
        {
            return new GetGeneratedRuleHandler
            {
                BusinessRuleEngine = _businessRuleEngineMock.Object,
                BusinessRuleGenerator = _businessRuleGeneratorMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _businessRuleEngineMock = new Mock<IBusinessRuleEngine>(MockBehavior.Strict);
            _businessRuleGeneratorMock = new Mock<IBusinessRuleGenerator>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldReturnNull_WhenTypeCannotBeResolved()
        {
            var request = new GetGeneratedRuleRequest
            {
                Namespace = RandomData.RandomString(10),
                Name = RandomData.RandomString(10)
            };

            _businessRuleEngineMock.Setup(x => x.GetType(string.Format("{0}.{1}", request.Namespace, request.Name)))
                .Returns((Type)null);

            var result = Sut.Handle(request);

            Assert.IsNull(result.Rule);
            _businessRuleEngineMock.Verify(x => x.GetType(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void Handle_ShouldReturnNull_WhenTypeIsNeitherQueryNorCommand()
        {
            var request = new GetGeneratedRuleRequest
            {
                Namespace = RandomData.RandomString(10),
                Name = RandomData.RandomString(10)
            };

            _businessRuleEngineMock.Setup(x => x.GetType(string.Format("{0}.{1}", request.Namespace, request.Name)))
                .Returns(typeof(int));

            var result = Sut.Handle(request);

            Assert.IsNull(result.Rule);
            _businessRuleEngineMock.Verify(x => x.GetType(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void Handle_ShouldReturnGeneratedCommand_WhenTypeIsCommand()
        {
            var request = new GetGeneratedRuleRequest
            {
                Namespace = RandomData.RandomString(10),
                Name = RandomData.RandomString(10),
                Context = new QueryContext { UserId = Guid.NewGuid() }
            };
            var rule = new BusinessRuleDescription();

            _businessRuleEngineMock.Setup(x => x.GetType(string.Format("{0}.{1}", request.Namespace, request.Name)))
                .Returns(typeof(TestCommand));
            _businessRuleGeneratorMock.Setup(x => x.GenerateCommandRule(typeof(TestCommand), request.Context.UserId))
                .Returns(rule);

            var result = Sut.Handle(request);

            Assert.AreEqual(rule, result.Rule);
            _businessRuleEngineMock.Verify(x => x.GetType(It.IsAny<string>()), Times.Once);
            _businessRuleGeneratorMock.Verify(x => x.GenerateCommandRule(It.IsAny<Type>(), It.IsAny<Guid>()), Times.Once);
            _businessRuleGeneratorMock.Verify(x => x.GenerateQueryRule(It.IsAny<Type>(), It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void Handle_ShouldReturnGeneratedQuery_WhenTypeIsQuery()
        {
            var request = new GetGeneratedRuleRequest
            {
                Namespace = RandomData.RandomString(10),
                Name = RandomData.RandomString(10),
                Context = new QueryContext { UserId = Guid.NewGuid() }
            };
            var rule = new BusinessRuleDescription();

            _businessRuleEngineMock.Setup(x => x.GetType(string.Format("{0}.{1}", request.Namespace, request.Name)))
                .Returns(typeof(TestQuery));
            _businessRuleGeneratorMock.Setup(x => x.GenerateQueryRule(typeof(TestQuery), request.Context.UserId))
                .Returns(rule);

            var result = Sut.Handle(request);

            Assert.AreEqual(rule, result.Rule);
            _businessRuleEngineMock.Verify(x => x.GetType(It.IsAny<string>()), Times.Once);
            _businessRuleGeneratorMock.Verify(x => x.GenerateCommandRule(It.IsAny<Type>(), It.IsAny<Guid>()), Times.Never);
            _businessRuleGeneratorMock.Verify(x => x.GenerateQueryRule(It.IsAny<Type>(), It.IsAny<Guid>()), Times.Once);
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
