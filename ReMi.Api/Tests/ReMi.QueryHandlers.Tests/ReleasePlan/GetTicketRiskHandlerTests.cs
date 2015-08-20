using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Queries.ReleasePlan;
using ReMi.QueryHandlers.ReleasePlan;
using System.Collections.Generic;

namespace ReMi.QueryHandlers.Tests.ReleasePlan
{
    public class GetTicketRiskHandlerTests : TestClassFor<GetTicketRiskHandler>
    {
        private Mock<IReleaseContentGateway> _releaseContentGateway;

        protected override GetTicketRiskHandler ConstructSystemUnderTest()
        {
            return new GetTicketRiskHandler
            {
                ReleaseContentGateway = () => _releaseContentGateway.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseContentGateway = new Mock<IReleaseContentGateway>(MockBehavior.Strict);
            _releaseContentGateway.Setup(x => x.Dispose());

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldGetTickets_WhenInvoked()
        {
            _releaseContentGateway.Setup(g => g.GetTicketRisk())
                .Returns(new List<EnumEntry>());

            Sut.Handle(new GetTicketRiskRequest());

            _releaseContentGateway.Verify(r => r.GetTicketRisk(), Times.Once());
        }
    }
}
