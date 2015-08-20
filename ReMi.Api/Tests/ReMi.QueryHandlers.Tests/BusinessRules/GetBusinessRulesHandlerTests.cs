using System.Linq;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.BusinessRules;
using ReMi.Common.Constants.BusinessRules;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.BusinessRules;
using ReMi.Queries.BusinessRules;
using ReMi.QueryHandlers.BusinessRules;

namespace ReMi.QueryHandlers.Tests.BusinessRules
{
    public class GetBusinessRulesHandlerTests : TestClassFor<GetBusinessRulesHandler>
    {
        private Mock<IBusinessRuleGateway> _businessRuleGatewayMock;

        protected override GetBusinessRulesHandler ConstructSystemUnderTest()
        {
            return new GetBusinessRulesHandler
            {
                BusinessRuleGatewayFactory = () => _businessRuleGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _businessRuleGatewayMock = new Mock<IBusinessRuleGateway>(MockBehavior.Strict);
            _businessRuleGatewayMock.Setup(x => x.Dispose());

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldGetBusinessRuleByName_WhenInvoked()
        {
            var request = new GetBusinessRulesRequest();

            _businessRuleGatewayMock.Setup(x => x.GetBusinessRules())
                .Returns(new []
                {
                    new BusinessRuleView
                    {
                        Group = BusinessRuleGroup.Release,
                        Name = RandomData.RandomString(10)
                    },
                    new BusinessRuleView
                    {
                        Group = BusinessRuleGroup.Permissions,
                        Name = RandomData.RandomString(10)
                    },
                    new BusinessRuleView
                    {
                        Group = BusinessRuleGroup.Release,
                        Name = RandomData.RandomString(10)
                    }
                });

            var result = Sut.Handle(request);

            Assert.AreEqual(2, result.Rules.Count());
            Assert.IsTrue(result.Rules.ContainsKey(BusinessRuleGroup.Release));
            Assert.IsTrue(result.Rules.ContainsKey(BusinessRuleGroup.Permissions));
            Assert.AreEqual(2, result.Rules[BusinessRuleGroup.Release].Count());
            _businessRuleGatewayMock.Verify(x => x.GetBusinessRules(), Times.Once);
        }
    }
}
