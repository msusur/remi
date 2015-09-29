using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessLogic.BusinessRules;
using ReMi.CommandHandlers.Auth;
using ReMi.Commands.Auth;
using ReMi.Common.Constants.BusinessRules;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandHandlers.Tests.Auth
{
    public class AssociateAccountsWithProductCommandHandlerTests : TestClassFor<AssociateAccountsWithProductCommandHandler>
    {
        private Mock<IAccountsGateway> _accountsGatewayMock;
        private Mock<IBusinessRuleEngine> _businessRuleEngineMock;

        protected override AssociateAccountsWithProductCommandHandler ConstructSystemUnderTest()
        {
            return new AssociateAccountsWithProductCommandHandler
            {
                AccountsGatewayFactory = () => _accountsGatewayMock.Object,
                BusinessRuleEngine = _businessRuleEngineMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _accountsGatewayMock = new Mock<IAccountsGateway>(MockBehavior.Strict);
            _businessRuleEngineMock = new Mock<IBusinessRuleEngine>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallGatewayToCreateAccount_WhenCalled()
        {
            var command = new AssociateAccountsWithProductCommand
            {
                Accounts = Builder<Account>.CreateListOfSize(5).Build(),
                ReleaseWindowId = Guid.NewGuid(),
                CommandContext = new CommandContext { UserId = Guid.NewGuid() }
            };
            var result = new TeamRoleRuleResult();
            var roleName = RandomData.RandomString(10);

            _accountsGatewayMock.Setup(x => x.AssociateAccountsWithProduct(
                It.Is<IEnumerable<string>>(s => s.SequenceEqual(command.Accounts.Select(a => a.Email))),
                command.ReleaseWindowId,
                It.Is<Func<string, TeamRoleRuleResult>>(f => f(roleName) == result)));
            _accountsGatewayMock.Setup(x => x.Dispose());

            _businessRuleEngineMock.Setup(x => x.Execute<TeamRoleRuleResult>(
                    command.CommandContext.UserId,
                    BusinessRuleConstants.Config.TeamRoleRule.ExternalId,
                    It.Is<IDictionary<string, object>>(d => ((string) d["roleName"]) == roleName)))
                .Returns(result);

            Sut.Handle(command);

            _accountsGatewayMock.Verify(x => x.AssociateAccountsWithProduct(
                It.IsAny<IEnumerable<string>>(), It.IsAny<Guid>(), It.IsAny<Func<string, TeamRoleRuleResult>>()),
                Times.Once);
            _accountsGatewayMock.Verify(x => x.Dispose(), Times.Once);
        }
    }
}
