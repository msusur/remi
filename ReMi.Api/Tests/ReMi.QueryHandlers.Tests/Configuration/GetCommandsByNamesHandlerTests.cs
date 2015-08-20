using System.Linq;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Api;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.DataAccess.Exceptions;
using ReMi.Queries.Configuration;
using ReMi.QueryHandlers.Configuration;

namespace ReMi.QueryHandlers.Tests.Configuration
{
    public class GetCommandsByNamesHandlerTests : TestClassFor<GetCommandsByNamesHandler>
    {
        private Mock<ICommandPermissionsGateway> _commandPermissionsGatewayMock;

        protected override GetCommandsByNamesHandler ConstructSystemUnderTest()
        {
            return new GetCommandsByNamesHandler
            {
                CommandPermissionsGatewayFactory = () => _commandPermissionsGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _commandPermissionsGatewayMock = new Mock<ICommandPermissionsGateway>();

            base.TestInitialize();
        }

        [Test]
        [ExpectedException(typeof(CommandListNotFoundException))]
        public void Handle_ShouldThrowException_WhenEmptyStringPassed()
        {
            var request = new GetCommandsByNamesRequest
            {
                Names = " "
            };

            Sut.Handle(request);
        }

        [Test]
        [ExpectedException(typeof(CommandListNotFoundException))]
        public void Handle_ShouldThrowException_WhenListOfEmptyNamesPassed()
        {
            var request = new GetCommandsByNamesRequest
            {
                Names = ",  , ,"
            };

            Sut.Handle(request);
        }

        [Test]
        public void Handle_Should_When()
        {
            var request = new GetCommandsByNamesRequest
            {
                Names = "name1,name2"
            };

            _commandPermissionsGatewayMock.Setup(o => o.GetCommands(false))
                .Returns(new[]
                {
                    new Command{Name = "name2"}, 
                    new Command{Name = "name3"}, 
                });

            var response = Sut.Handle(request);

            Assert.IsNotNull(response);
            Assert.AreEqual(1, response.Commands.Count());
            Assert.AreEqual("name2", response.Commands.First().Name);
        }
    }
}
