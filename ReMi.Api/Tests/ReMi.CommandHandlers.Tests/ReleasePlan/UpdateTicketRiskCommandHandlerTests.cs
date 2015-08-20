using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.CommandHandlers.ReleasePlan;
using ReMi.Commands.ReleasePlan;
using ReMi.Common.Constants.ReleasePlan;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Events.ReleasePlan;

namespace ReMi.CommandHandlers.Tests.ReleasePlan
{
    [TestFixture]
    public class UpdateTicketRiskCommandHandlerTests : TestClassFor<UpdateTicketRiskCommandHandler>
    {
        private Mock<IReleaseContentGateway> _releaseContentGatewayMock;
        private Mock<IMappingEngine> _mapperMock;
        private Mock<IPublishEvent> _eventPublisherMock; 

        protected override UpdateTicketRiskCommandHandler ConstructSystemUnderTest()
        {
            return new UpdateTicketRiskCommandHandler
            {
                Mapper = _mapperMock.Object,
                ReleaseContentGateway = () => _releaseContentGatewayMock.Object,
                EventPublisher = _eventPublisherMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseContentGatewayMock = new Mock<IReleaseContentGateway>();
            _mapperMock = new Mock<IMappingEngine>();
            _eventPublisherMock = new Mock<IPublishEvent>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallGatway_WhenCommandHandled()
        {
            var command = new UpdateTicketRiskCommand();
            var input = new ReleaseContentTicket
            {
                LastChangedByAccount = Guid.NewGuid(),
                Risk = RandomData.RandomEnum<TicketRisk>(),
                TicketId = Guid.NewGuid(),
                TicketName = RandomData.RandomString(10)
            };
            _mapperMock.Setup(x => x.Map<UpdateTicketRiskCommand, ReleaseContentTicket>(command))
                .Returns(input);
            _releaseContentGatewayMock.Setup(x => x.GetTicketInformations(It.Is<IEnumerable<Guid>>(i => i.First() == input.TicketId)))
                .Returns(new[] { input });
           
            Sut.Handle(command);

            _releaseContentGatewayMock.Verify(x => x.AddOrUpdateTicketRisk(input));
            _releaseContentGatewayMock.Verify(x => x.GetTicketInformations(It.IsAny<IEnumerable<Guid>>()), Times.Once);
            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<TicketChangedEvent>(u => u.Tickets.Count == 1 && u.Tickets.Contains(input))));
        }
    }
}
