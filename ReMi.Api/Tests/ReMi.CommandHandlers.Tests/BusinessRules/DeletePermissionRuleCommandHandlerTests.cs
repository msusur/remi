using System;
using Moq;
using NUnit.Framework;
using ReMi.CommandHandlers.BusinessRules;
using ReMi.Commands.BusinessRules;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.BusinessRules;

namespace ReMi.CommandHandlers.Tests.BusinessRules
{
    public class DeletePermissionRuleCommandHandlerTests : TestClassFor<DeletePermissionRuleCommandHandler>
    {
        private Mock<IBusinessRuleGateway> _businessRuleGatewayMock;

        protected override DeletePermissionRuleCommandHandler ConstructSystemUnderTest()
        {
            return new DeletePermissionRuleCommandHandler
            {
                BusinessRuleGatewayFacotory = () => _businessRuleGatewayMock.Object,
            };
        }

        protected override void TestInitialize()
        {
            _businessRuleGatewayMock = new Mock<IBusinessRuleGateway>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallDeleteMethod_WhenInvoked()
        {
            var command = new DeletePermissionRuleCommand
            {
                RuleId = Guid.NewGuid()
            };
            Sut.Handle(command);

            _businessRuleGatewayMock.Verify(x => x.DeleteBusinessRule(command.RuleId), Times.Once);
        }
    }
}
