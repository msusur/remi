using System;
using Moq;
using NUnit.Framework;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseExecution;
using ReMi.Queries.ReleaseExecution;
using ReMi.QueryHandlers.ReleaseExecution;

namespace ReMi.QueryHandlers.Tests.ReleaseExecution
{
    public class GetSignOffsHandlerTests : TestClassFor<GetSignOffsHandler>
    {
        private Mock<ISignOffGateway> _signOffGatewayMock;

        protected override GetSignOffsHandler ConstructSystemUnderTest()
        {
            return new GetSignOffsHandler
            {
                SignOffGatewayFactory = () => _signOffGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _signOffGatewayMock = new Mock<ISignOffGateway>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallGatewayToGetSignOffs()
        {
            var request = new GetSignOffsRequest
            {
                ReleaseWindowId = Guid.NewGuid()
            };

            Sut.Handle(request);

            _signOffGatewayMock.Verify(g => g.GetSignOffs(request.ReleaseWindowId));
        }
    }
}
