using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.HelpDesk;
using ReMi.BusinessEntities.Products;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.CommandHandlers.ReleasePlan;
using ReMi.Commands.ReleasePlan;
using ReMi.Common.Utils;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Plugins.Data.HelpDesk;
using ReMi.Contracts.Plugins.Services.HelpDesk;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Events.ReleasePlan;

namespace ReMi.CommandHandlers.Tests.HelpDesk
{
    public class CreateHelpDeskTaskCommandHandlerTests
        : TestClassFor<CreateHelpDeskTaskCommandHandler>
    {
        private Mock<IPublishEvent> _eventPublisherMock;
        private Mock<IReleaseTaskGateway> _releaseTaskGatewayMock;
        private Mock<IHelpDeskService> _helpDeskRequestMock;
        private Mock<IReleaseWindowGateway> _releaseWindowGatewayMock;
        private Mock<IMappingEngine> _mappingEngineMock;
        private Mock<IApplicationSettings> _applicationSettingsMock;
        private Mock<IProductGateway> _productGatewayMock;

        protected override CreateHelpDeskTaskCommandHandler ConstructSystemUnderTest()
        {
            return new CreateHelpDeskTaskCommandHandler
            {
                EventPublisher = _eventPublisherMock.Object,
                MappingEngine = _mappingEngineMock.Object,
                ReleaseTaskGatewayFactory = () => _releaseTaskGatewayMock.Object,
                ReleaseWindowGatewayFactory = () => _releaseWindowGatewayMock.Object,
                HelpDeskService = _helpDeskRequestMock.Object,
                ApplicationSettings = _applicationSettingsMock.Object,
                ProductGatewayFactory = () => _productGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _eventPublisherMock = new Mock<IPublishEvent>();
            _mappingEngineMock = new Mock<IMappingEngine>();
            _releaseTaskGatewayMock = new Mock<IReleaseTaskGateway>();
            _releaseWindowGatewayMock = new Mock<IReleaseWindowGateway>();
            _helpDeskRequestMock = new Mock<IHelpDeskService>();
            _applicationSettingsMock = new Mock<IApplicationSettings>(MockBehavior.Strict);
            _productGatewayMock = new Mock<IProductGateway>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCreateHelpDeskTicket_WhenCallHelpDeskService()
        {
            var data = new
            {
                Description = RandomData.RandomString(1, 150),
                Type = RandomData.RandomString(1, 20),
                Subject = RandomData.RandomString(1, 150),
                Url = RandomData.RandomString(1, 150),
                Id = RandomData.RandomString(10),
            };

            var task = new ReleaseTask
            {
                Description = data.Description,
                ExternalId = Guid.NewGuid(),
                ReleaseWindowId = Guid.NewGuid(),
                Type = data.Type,
                CreateHelpDeskTicket = true
            };
            var productId = Guid.NewGuid();
            _helpDeskRequestMock.Setup(x => x.CreateTicket(It.IsAny<HelpDeskTicket>(), It.IsAny<IEnumerable<Guid>>()))
                .Returns(new HelpDeskTicket
                {
                    Description = data.Description,
                    Subject = data.Subject,
                    Id = data.Id,
                    Url = data.Url
                });
            _releaseWindowGatewayMock.Setup(x => x.GetByExternalId(task.ReleaseWindowId, false, It.IsAny<bool>()))
                .Returns(Builder<ReleaseWindow>.CreateNew().With(x => x.ExternalId, task.ReleaseWindowId).Build());
            _releaseTaskGatewayMock.Setup(x => x.GetReleaseTask(task.ExternalId))
                .Returns(new ReleaseTask());
            _applicationSettingsMock.SetupGet(x => x.FrontEndUrl).Returns("frontEndUrl/");
            _productGatewayMock.Setup(x => x.GetProducts(task.ReleaseWindowId))
                .Returns(new[] { new Product { ExternalId = productId } });

            var ticket = new HelpDeskTicket
            {
                Description = data.Description,
                Comment = data.Description
            };

            _mappingEngineMock.Setup(m => m.Map<ReleaseTask, HelpDeskTicket>(task)).Returns(ticket);

            _mappingEngineMock.Setup(m => m.Map<HelpDeskTicket, HelpDeskTask>(It.Is<HelpDeskTicket>(x => x.Id == data.Id)))
                .Returns(new HelpDeskTask());

            Sut.Handle(new CreateHelpDeskTaskCommand
            {
                ReleaseTask = task
            });

            _mappingEngineMock.Verify(m => m.Map<ReleaseTask, HelpDeskTicket>(task));
            _mappingEngineMock.Verify(m => m.Map<HelpDeskTicket, HelpDeskTask>(It.Is<HelpDeskTicket>(x => x.Id == data.Id)));
            _helpDeskRequestMock.Verify(x => x.CreateTicket(It.Is<HelpDeskTicket>(
                m => m.Description == data.Description
                    && m.Comment == string.Format("{0}{1}{2}{3}",
                        data.Description, Environment.NewLine,
                        "frontEndUrl/release?releaseWindowId=", task.ReleaseWindowId)),
                It.Is<IEnumerable<Guid>>(id => id.First() == productId)));
            _releaseTaskGatewayMock.Verify(x => x.AssignHelpDeskTicket(task, data.Id.ToString(), data.Url));
            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<HelpDeskTaskCreatedEvent>(
                            x => x.ReleaseTaskId == task.ExternalId && x.ReleaseWindowId == task.ReleaseWindowId)));
        }

        [Test]
        public void Handle_ShouldNotCreateHelpDeskTicket_WhenHelpDeskTicketDefined()
        {
            var data = new
            {
                Description = RandomData.RandomString(1, 150),
                Type = RandomData.RandomString(1, 20),
                Subject = RandomData.RandomString(1, 150),
                Url = RandomData.RandomString(1, 150),
                Id = RandomData.RandomInt(1, int.MaxValue),
            };

            var task = new ReleaseTask
            {
                Description = data.Description,
                ExternalId = Guid.NewGuid(),
                ReleaseWindowId = Guid.NewGuid(),
                Type = data.Type,
                CreateHelpDeskTicket = true
            };

            _releaseTaskGatewayMock.Setup(x => x.GetReleaseTask(task.ExternalId))
                .Returns(new ReleaseTask { HelpDeskTicketReference = "100" });

            Sut.Handle(new CreateHelpDeskTaskCommand
            {
                ReleaseTask = task
            });

            _helpDeskRequestMock.Verify(x => x.CreateTicket(It.IsAny<HelpDeskTicket>(), It.IsAny<IEnumerable<Guid>>()), Times.Never());
            _releaseTaskGatewayMock.Verify(x => x.GetReleaseTask(task.ExternalId));
        }
    }
}
