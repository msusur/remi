using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.CommandHandlers.ReleasePlan;
using ReMi.Commands.ReleaseExecution;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Services.ReleaseContent;

namespace ReMi.CommandHandlers.Tests.ReleasePlan
{

    public class UpdateTicketLabelsCommandHandlerTest : TestClassFor<UpdateTicketLabelsCommandHandler>
    {
        private Mock<IReleaseContent> _releaseContentMock;
        private Mock<IMappingEngine> _mappingEngineMock;


        protected override UpdateTicketLabelsCommandHandler ConstructSystemUnderTest()
        {
            return new UpdateTicketLabelsCommandHandler
            {
                ReleaseContent = _releaseContentMock.Object,
                MappingEngine = _mappingEngineMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseContentMock = new Mock<IReleaseContent>();
            _mappingEngineMock = new Mock<IMappingEngine>();
            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallAddLabel_WhenCommandIsBeingHandled()
        {
            var updateTicketLabelsCommand = Builder<UpdateTicketLabelsCommand>.CreateNew()
                .With(x => x.Tickets, Builder<ReleaseContentTicket>.CreateListOfSize(5)
                    .All()
                    .Do(x => x.TicketId = Guid.NewGuid())
                    .Build())
                .With(x => x.PackageId, Guid.NewGuid())
                .Build();
            _mappingEngineMock.Setup(x => x.Map<IEnumerable<ReleaseContentTicket>, IEnumerable<Contracts.Plugins.Data.ReleaseContent.ReleaseContentTicket>>(updateTicketLabelsCommand.Tickets))
                .Returns((IEnumerable<ReleaseContentTicket> t) => t.Select(x => new Contracts.Plugins.Data.ReleaseContent.ReleaseContentTicket { TicketId = x.TicketId }));

            Sut.Handle(updateTicketLabelsCommand);

            _releaseContentMock.Verify(
                j => j.UpdateTicket(
                    It.Is<IEnumerable<Contracts.Plugins.Data.ReleaseContent.ReleaseContentTicket>>(
                        s => s.Count() == 5 && updateTicketLabelsCommand.Tickets.All(x => s.Any(y => y.TicketId == x.TicketId))),
                    It.Is<Guid>(s => s == updateTicketLabelsCommand.PackageId)),
                    Times.Once);
        }
    }

}
