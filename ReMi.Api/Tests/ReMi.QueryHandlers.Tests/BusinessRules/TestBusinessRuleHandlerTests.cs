using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.BusinessRules;
using ReMi.BusinessLogic.BusinessRules;
using ReMi.TestUtils.UnitTests;
using ReMi.Queries.BusinessRules;
using ReMi.QueryHandlers.BusinessRules;

namespace ReMi.QueryHandlers.Tests.BusinessRules
{
    public class TestBusinessRuleHandlerTests : TestClassFor<TestBusinessRuleHandler>
    {
        private Mock<IBusinessRuleEngine> _businessRuleEngineMock;

        protected override TestBusinessRuleHandler ConstructSystemUnderTest()
        {
            return new TestBusinessRuleHandler
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
        public void Handle_ShouldGetResultFromBusinessRuleEngine_WhenInvoked()
        {
            var request = new TestBusinessRuleRequest
            {
                Rule = new BusinessRuleDescription()
            };

            _businessRuleEngineMock.Setup(x => x.Test(request.Rule))
                .Returns("test result");

            var result = Sut.Handle(request);

            Assert.AreEqual("test result", result.Result);

            _businessRuleEngineMock.Verify(x => x.Test(It.IsAny<BusinessRuleDescription>()), Times.Once);
        }
    }
}
