using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Products;
using ReMi.CommandHandlers.ReleasePlan;
using ReMi.Commands.ReleasePlan;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Plugins.Services.HelpDesk;
using ReMi.DataAccess.BusinessEntityGateways.Products;

namespace ReMi.CommandHandlers.Tests.ReleasePlan
{
    public class DeleteHelpDeskTaskCommandHandlerTests : TestClassFor<DeleteHelpDeskTaskCommandHandler>
    {
        private Mock<IHelpDeskService> _helpDeskRequestMock;
        private Mock<IProductGateway> _productGatewayMock;

        protected override DeleteHelpDeskTaskCommandHandler ConstructSystemUnderTest()
        {
            return new DeleteHelpDeskTaskCommandHandler
            {
                HelpDeskService = _helpDeskRequestMock.Object,
                ProductGatewayFactory = () => _productGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _helpDeskRequestMock = new Mock<IHelpDeskService>();
            _productGatewayMock = new Mock<IProductGateway>();

            base.TestInitialize();
        }

        [Test]
        public void DeleteReleaseTask_ShouldCallGatewayMethodAndDisposeIt_WhenCallCallGateway()
        {
            var command = new DeleteHelpDeskTaskCommand
            {
                CommandContext = new CommandContext(),
                HelpDeskTicketRef = RandomData.RandomString(10),
                ReleaseWindowId = Guid.NewGuid()
            };
            var productId = Guid.NewGuid();

            _productGatewayMock.Setup(x => x.GetProducts(command.ReleaseWindowId))
                .Returns(new[] {new Product { ExternalId = productId } });

            Sut.Handle(command);

            _helpDeskRequestMock.Verify(x => x.DeleteTicket(command.HelpDeskTicketRef, It.Is<IEnumerable<Guid>>(id => id.First() == productId)));
        }
    }
}
