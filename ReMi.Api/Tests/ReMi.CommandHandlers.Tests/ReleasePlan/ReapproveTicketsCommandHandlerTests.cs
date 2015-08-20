using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.CommandHandlers.ReleasePlan;
using ReMi.Commands.ReleasePlan;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Queries.ReleasePlan;

namespace ReMi.CommandHandlers.Tests.ReleasePlan
{
    public class ReapproveTicketsCommandHandlerTests : TestClassFor<ReapproveTicketsCommandHandler>
    {
        private Mock<IReleaseContentGateway> _releaseContentGatewayMock;
        private Mock<IHandleQuery<GetReleaseContentInformationRequest, GetReleaseContentInformationResponse>> _getReleaseContentInformationQueryMock;

        protected override ReapproveTicketsCommandHandler ConstructSystemUnderTest()
        {
            return new ReapproveTicketsCommandHandler
            {
                ReleaseContentGatewayFactory = () => _releaseContentGatewayMock.Object,
                GetReleaseContentInformationQuery = _getReleaseContentInformationQueryMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseContentGatewayMock = new Mock<IReleaseContentGateway>(MockBehavior.Strict);
            _releaseContentGatewayMock.Setup(x => x.Dispose());
            _getReleaseContentInformationQueryMock =
                new Mock<IHandleQuery<GetReleaseContentInformationRequest, GetReleaseContentInformationResponse>>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldRemoveExistingTicketsGetNewContentAndIncludeAllInThisRelease_WhenInvoked()
        {
            var command = new ReapproveTicketsCommand
            {
                CommandContext = new CommandContext { UserId = Guid.NewGuid() },
                ReleaseWindowId = Guid.NewGuid()
            };
            var content = new List<ReleaseContentTicket>
            {
                new ReleaseContentTicket { IncludeToReleaseNotes = true },
                new ReleaseContentTicket { IncludeToReleaseNotes = false }
            };
            _releaseContentGatewayMock.Setup(x => x.RemoveTicketsFromRelease(command.ReleaseWindowId));
            _getReleaseContentInformationQueryMock.Setup(x => x.Handle(It.Is<GetReleaseContentInformationRequest>(
                    r => r.ReleaseWindowId == command.ReleaseWindowId)))
                .Returns(new GetReleaseContentInformationResponse { Content = content });
            _releaseContentGatewayMock.Setup(g => g.AddOrUpdateTickets(content, command.CommandContext.UserId, command.ReleaseWindowId));

            Sut.Handle(command);

            _releaseContentGatewayMock.Verify(x => x.RemoveTicketsFromRelease(It.IsAny<Guid>()), Times.Once);
            _getReleaseContentInformationQueryMock.Verify(x => x.Handle(It.IsAny<GetReleaseContentInformationRequest>()), Times.Once());
            _releaseContentGatewayMock.Verify(
                    g => g.AddOrUpdateTickets(
                        It.Is<IEnumerable<ReleaseContentTicket>>(c => c.All(j => j.IncludeToReleaseNotes)),
                        It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
        }
    }
}
