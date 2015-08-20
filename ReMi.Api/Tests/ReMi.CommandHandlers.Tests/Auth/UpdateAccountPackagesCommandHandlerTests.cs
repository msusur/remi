using Moq;
using NUnit.Framework;
using ReMi.CommandHandlers.Auth;
using ReMi.Commands.Auth;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using System;
using System.Collections.Generic;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandHandlers.Tests.Auth
{
    public class UpdateAccountPackagesCommandHandlerTests : TestClassFor<UpdateAccountPackagesCommandHandler>
    {
        private Mock<IAccountsGateway> _accountsGatewayMock;

        protected override UpdateAccountPackagesCommandHandler ConstructSystemUnderTest()
        {
            return new UpdateAccountPackagesCommandHandler
            {
                AccountsGatewayFactory = () => _accountsGatewayMock.Object,
            };
        }

        protected override void TestInitialize()
        {
            _accountsGatewayMock = new Mock<IAccountsGateway>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallGatewayToUpdateAccountPackages_WhenCalled()
        {
            var command = new UpdateAccountPackagesCommand
            {
                AccountId = Guid.NewGuid(),
                PackageIds = new[] { Guid.NewGuid() },
                DefaultPackageId = new Guid()
            };
            _accountsGatewayMock.Setup(x => x.UpdateAccountPackages(
                command.AccountId,
                command.PackageIds,
                command.DefaultPackageId));
            _accountsGatewayMock.Setup(x => x.Dispose());

            Sut.Handle(command);

            _accountsGatewayMock.Verify(x => x.UpdateAccountPackages(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>(), It.IsAny<Guid>()), Times.Once);
        }
    }
}
