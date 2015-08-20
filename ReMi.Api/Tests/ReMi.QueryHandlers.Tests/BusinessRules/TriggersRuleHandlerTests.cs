using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessLogic.BusinessRules;
using ReMi.Common.Constants.BusinessRules;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Queries.BusinessRules;
using ReMi.QueryHandlers.BusinessRules;

namespace ReMi.QueryHandlers.Tests.BusinessRules
{
    public class TriggersRuleHandlerTests : TestClassFor<TriggerBusinessRuleHandler>
    {
        private Mock<IBusinessRuleEngine> _businessRuleEngineMock;

        protected override TriggerBusinessRuleHandler ConstructSystemUnderTest()
        {
            return new TriggerBusinessRuleHandler
            {
                BusinessRuleEngine = _businessRuleEngineMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _businessRuleEngineMock = new Mock<IBusinessRuleEngine>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldGetBusinessRuleByExternalId_WhenExternalIdIsNotEmpty()
        {
            var request = new TriggerBusinessRuleRequest
            {
                Context = new QueryContext { UserId = Guid.NewGuid() },
                Group = BusinessRuleGroup.Permissions,
                Rule = "RuleName",
                ExternalId = Guid.NewGuid(),
                Parameters = new Dictionary<string, string>()
            };

            _businessRuleEngineMock.Setup(x => x.Execute(request.Context.UserId, request.ExternalId, request.Parameters))
                .Returns(string.Empty);

            var result = Sut.Handle(request);


            _businessRuleEngineMock.Verify(x => x.Execute(request.Context.UserId, request.ExternalId, request.Parameters), Times.Once);
            Assert.AreEqual(result.Result, string.Empty);
        }

        [Test]
        public void Handle_ShouldGetBusinessRuleByName_WhenExternalIdIsEmpty()
        {
            var request = new TriggerBusinessRuleRequest
            {
                Context = new QueryContext { UserId = Guid.NewGuid() },
                Group = BusinessRuleGroup.Permissions,
                Rule = "RuleName",
                ExternalId = Guid.Empty,
                Parameters = new Dictionary<string, string>()
            };

            _businessRuleEngineMock.Setup(x => x.Execute(request.Context.UserId, request.Group, request.Rule, request.Parameters))
                .Returns(string.Empty);

            var result = Sut.Handle(request);


            _businessRuleEngineMock.Verify(x => x.Execute(request.Context.UserId, request.Group, request.Rule, request.Parameters), Times.Once);
            Assert.AreEqual(result.Result, string.Empty);
        }
    }
}
