using Moq;
using NUnit.Framework;
using ReMi.CommandHandlers.Auth;
using ReMi.Commands.Auth;
using ReMi.Common.Utils;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using System;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandHandlers.Tests.Auth
{
    public class ProlongSessionHandlerTests : TestClassFor<ProlongSessionHandler>
    {
        private Mock<IAccountsGateway> _accountsGatewayMock;
        private Mock<IApplicationSettings> _applicationSettingsMock;

        protected override ProlongSessionHandler ConstructSystemUnderTest()
        {
            return new ProlongSessionHandler
            {
                AccountsGatewayFactory = () => _accountsGatewayMock.Object,
                ApplicationSettings = _applicationSettingsMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _accountsGatewayMock = new Mock<IAccountsGateway>();
            _applicationSettingsMock = new Mock<IApplicationSettings>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallGatewayToProlongSession()
        {
            _applicationSettingsMock.SetupGet(x => x.SessionDuration).Returns(15);

            var command = new ProlongSessionCommand {SessionId = Guid.NewGuid()};

            Sut.Handle(command);

            _accountsGatewayMock.Verify(x => x.ProlongSession(command.SessionId, 15));
        }
    }
}
