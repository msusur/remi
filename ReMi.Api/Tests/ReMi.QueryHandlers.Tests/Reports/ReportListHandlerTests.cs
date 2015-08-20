using Moq;
using NUnit.Framework;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.Reports;
using ReMi.Queries.Reports;
using ReMi.QueryHandlers.Reports;

namespace ReMi.QueryHandlers.Tests.Reports
{
    public class ReportListHandlerTests : TestClassFor<ReportListHandler>
    {
        private Mock<IReportGateway> _reportGatewayMock;

        protected override ReportListHandler ConstructSystemUnderTest()
        {
            return new ReportListHandler
            {
                ReportGatewayFactory = () => _reportGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _reportGatewayMock = new Mock<IReportGateway>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallReportGateway_WhenInvoked()
        {
            Sut.Handle(new ReportsListRequest());

            _reportGatewayMock.Verify(s=>s.GetReportDescriptions());
        }
    }
}
