using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Products;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.CommandHandlers.ReleasePlan;
using ReMi.Commands.ReleasePlan;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Plugins.Data.HelpDesk;
using ReMi.Contracts.Plugins.Services.HelpDesk;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;

namespace ReMi.CommandHandlers.Tests.HelpDesk
{
    public class UpdateHelpDeskTaskCommandHandlerTests
        : TestClassFor<UpdateHelpDeskTaskCommandHandler>
    {
        private Mock<IHelpDeskService> _helpDeskServiceMock;
        private Mock<IReleaseWindowGateway> _releaseWindowGatewayMock;
        private Mock<IPublishEvent> _eventPublisherMock;
        private Mock<IMappingEngine> _mapperMock;
        private Mock<IProductGateway> _productGatewayMock;

        protected override UpdateHelpDeskTaskCommandHandler ConstructSystemUnderTest()
        {
            return new UpdateHelpDeskTaskCommandHandler
            {
                MappingEngine = _mapperMock.Object,
                EventPublisher = _eventPublisherMock.Object,
                ReleaseWindowGatewayFactory = () => _releaseWindowGatewayMock.Object,
                HelpDeskService = _helpDeskServiceMock.Object,
                ProductGatewayFactory = () => _productGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _helpDeskServiceMock = new Mock<IHelpDeskService>();
            _releaseWindowGatewayMock = new Mock<IReleaseWindowGateway>();
            _eventPublisherMock = new Mock<IPublishEvent>();
            _mapperMock = new Mock<IMappingEngine>();
            _productGatewayMock = new Mock<IProductGateway>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldSendUpdateHelpDeskTicketInGateway_WhenInvoked()
        {
            var task = new ReleaseTask
            {
                HelpDeskTicketReference = "123",
                Description = "desc",
                ReleaseWindowId = Guid.NewGuid(),
            };
            var productId = Guid.NewGuid();
            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(task.ReleaseWindowId, false, It.IsAny<bool>()))
                .Returns(Builder<ReleaseWindow>.CreateNew()
                    .Build());
            _mapperMock.Setup(m => m.Map<ReleaseTask, HelpDeskTicket>(task)).Returns(new HelpDeskTicket
            {
                Description = "desc",
                Id = "123"
            });
            _productGatewayMock.Setup(x => x.GetProducts(task.ReleaseWindowId))
                .Returns(new[] { new Product { ExternalId = productId } });

            Sut.Handle(new UpdateHelpDeskTaskCommand { ReleaseTask = task });

            _mapperMock.Verify(m => m.Map<ReleaseTask, HelpDeskTicket>(task));
            _helpDeskServiceMock.Verify(x => x.UpdateTicket(It.Is<HelpDeskTicket>(m => m.Description == "desc" && m.Id == "123"),
                It.Is<IEnumerable<Guid>>(id => id.First() == productId)));
            _releaseWindowGatewayMock.Verify(x => x.GetByExternalId(task.ReleaseWindowId, false, It.IsAny<bool>()));
        }
    }
}
