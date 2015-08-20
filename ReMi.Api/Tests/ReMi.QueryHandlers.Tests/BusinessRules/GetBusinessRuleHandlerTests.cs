using System;
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
    public class GetBusinessRuleHandlerTests : TestClassFor<GetBusinessRuleHandler>
    {
        private Mock<IBusinessRuleGateway> _businessRuleGatewayMock;

        protected override GetBusinessRuleHandler ConstructSystemUnderTest()
        {
            return new GetBusinessRuleHandler
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
        public void Handle_ShouldGetBusinessRuleByName_WhenExternalIdIsNull()
        {
            var request = new GetBusinessRuleRequest
            {
                Group = RandomData.RandomEnum<BusinessRuleGroup>(),
                Name = RandomData.RandomString(10)
            };

            _businessRuleGatewayMock.Setup(x => x.GetBusinessRule(request.Group, request.Name))
                .Returns(new BusinessRuleDescription
                {
                    Name = request.Name
                });

            Sut.Handle(request);


            _businessRuleGatewayMock.Verify(x => x.GetBusinessRule(request.Group, request.Name), Times.Once);
        }

        [Test]
        public void Handle_ShouldGetBusinessRuleById_WhenExternalIdIsNotNull()
        {
            var request = new GetBusinessRuleRequest
            {
                ExternalId = Guid.NewGuid()
            };

            _businessRuleGatewayMock.Setup(x => x.GetBusinessRule(request.ExternalId.Value))
                .Returns(new BusinessRuleDescription
                {
                    Name = request.Name
                });

            Sut.Handle(request);


            _businessRuleGatewayMock.Verify(x => x.GetBusinessRule(request.ExternalId.Value), Times.Once);
        }
    }
}
