using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Api;
using ReMi.CommandHandlers.Api;
using ReMi.Commands;
using ReMi.DataAccess.BusinessEntityGateways.Api;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandHandlers.Tests.Api
{
    public class UpdateApiDescriptionHandlerTests : TestClassFor<UpdateApiDescriptionHandler>
    {
        private Mock<IApiDescriptionGateway> _apiDecriptionGatewayMock;

        protected override UpdateApiDescriptionHandler ConstructSystemUnderTest()
        {
            return new UpdateApiDescriptionHandler
            {
                ApiDescriptionGatewayFactory = () => _apiDecriptionGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _apiDecriptionGatewayMock = new Mock<IApiDescriptionGateway>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallGatewayToUpdateReleaseDescription()
        {
            var command = new UpdateApiDescriptionCommand
            {
                ApiDescription = new ApiDescription
                {
                    Description = RandomData.RandomString(12, 14),
                    Method = "GET",
                    Url = RandomData.RandomString(5, 19)
                }
            };

            Sut.Handle(command);

            _apiDecriptionGatewayMock.Verify(x => x.UpdateApiDescription(command.ApiDescription));
        }
    }
}
