using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.BusinessRules;
using ReMi.BusinessEntities.Exceptions;
using ReMi.BusinessLogic.BusinessRules;
using ReMi.CommandHandlers.BusinessRules;
using ReMi.Commands.BusinessRules;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.BusinessRules;

namespace ReMi.CommandHandlers.Tests.BusinessRules
{
    public class SaveQueryPermissionRuleCommandHandlerTests : TestClassFor<SaveQueryPermissionRuleCommandHandler>
    {
        private Mock<IBusinessRuleGateway> _businessRuleGatewayMock;
        private Mock<IBusinessRuleEngine> _businessRuleEngineMock;

        protected override SaveQueryPermissionRuleCommandHandler ConstructSystemUnderTest()
        {
            return new SaveQueryPermissionRuleCommandHandler
            {
                BusinessRuleEngine = _businessRuleEngineMock.Object,
                BusinessRuleGatewayFactory = () => _businessRuleGatewayMock.Object,
            };
        }

        protected override void TestInitialize()
        {
            _businessRuleGatewayMock = new Mock<IBusinessRuleGateway>(MockBehavior.Strict);
            _businessRuleEngineMock = new Mock<IBusinessRuleEngine>(MockBehavior.Strict);
            _businessRuleGatewayMock.Setup(x => x.Dispose());

            base.TestInitialize();
        }

        [Test]
        [ExpectedException(typeof(BusinessRuleCompilationException))]
        public void Handle_ShouldThrowCompilationException_WhenTestResultIsNotBool()
        {
            var command = new SaveQueryPermissionRuleCommand
            {
                Rule = new BusinessRuleDescription
                {
                    ExternalId = Guid.NewGuid()
                },
                QueryId = RandomData.RandomInt(int.MaxValue)
            };

            // return not bool value should cause exception
            _businessRuleEngineMock.Setup(x => x.Test(command.Rule))
                .Returns(string.Empty);

            Sut.Handle(command);
        }

        [Test]
        public void Handle_ShouldCreateAndAssignRuleToCommand_WhenRuleNotExist()
        {
            var command = new SaveQueryPermissionRuleCommand
            {
                Rule = new BusinessRuleDescription
                {
                    ExternalId = Guid.NewGuid()
                },
                QueryId = RandomData.RandomInt(int.MaxValue)
            };

            // return not bool value should cause exception
            _businessRuleEngineMock.Setup(x => x.Test(command.Rule))
                .Returns(true);
            _businessRuleGatewayMock.Setup(x => x.GetBusinessRule(command.Rule.ExternalId))
                .Returns((BusinessRuleDescription)null);
            _businessRuleGatewayMock.Setup(x => x.CreateBusinessRule(command.Rule));
            _businessRuleGatewayMock.Setup(x => x.AddRuleToQuery(command.Rule.ExternalId, command.QueryId));

            Sut.Handle(command);

            _businessRuleEngineMock.Verify(x => x.Test(command.Rule), Times.Once());
            _businessRuleGatewayMock.Verify(x => x.CreateBusinessRule(It.IsAny<BusinessRuleDescription>()), Times.Once);
            _businessRuleGatewayMock.Verify(x => x.UpdateRuleScript(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
            _businessRuleGatewayMock.Verify(x => x.UpdateTestData(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
            _businessRuleGatewayMock.Verify(x => x.UpdateAccountTestData(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
            _businessRuleGatewayMock.Verify(x => x.AddRuleToQuery(It.IsAny<Guid>(), It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void Handle_ShouldUpdateAndAssignRuleToCommand_WhenRuleExists()
        {
            var command = new SaveQueryPermissionRuleCommand
            {
                Rule = new BusinessRuleDescription
                {
                    ExternalId = Guid.NewGuid(),
                    Script = "script",
                    AccountTestData = new BusinessRuleAccountTestData
                    {
                        ExternalId = Guid.NewGuid(),
                        JsonData = "account json"
                    },
                    Parameters = new List<BusinessRuleParameter>
                    {
                        new BusinessRuleParameter
                        {
                            ExternalId = Guid.Empty,
                            TestData = new BusinessRuleTestData
                            {
                                ExternalId = Guid.NewGuid(),
                                JsonData = "parameter json"
                            }
                        }
                    }
                },
                QueryId = RandomData.RandomInt(int.MaxValue)
            };

            // return not bool value should cause exception
            _businessRuleEngineMock.Setup(x => x.Test(command.Rule))
                .Returns(true);
            _businessRuleGatewayMock.Setup(x => x.GetBusinessRule(command.Rule.ExternalId))
                .Returns(command.Rule);
            _businessRuleGatewayMock.Setup(x => x.UpdateRuleScript(command.Rule.ExternalId, command.Rule.Script));
            _businessRuleGatewayMock.Setup(x => x.UpdateTestData(command.Rule.Parameters.First().TestData.ExternalId, command.Rule.Parameters.First().TestData.JsonData));
            _businessRuleGatewayMock.Setup(x => x.UpdateAccountTestData(command.Rule.AccountTestData.ExternalId, command.Rule.AccountTestData.JsonData));

            Sut.Handle(command);

            _businessRuleEngineMock.Verify(x => x.Test(command.Rule), Times.Once());
            _businessRuleGatewayMock.Verify(x => x.CreateBusinessRule(It.IsAny<BusinessRuleDescription>()), Times.Never);
            _businessRuleGatewayMock.Verify(x => x.UpdateRuleScript(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
            _businessRuleGatewayMock.Verify(x => x.UpdateTestData(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
            _businessRuleGatewayMock.Verify(x => x.UpdateAccountTestData(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
            _businessRuleGatewayMock.Verify(x => x.AddRuleToQuery(It.IsAny<Guid>(), It.IsAny<int>()), Times.Never);
        }
    }
}
