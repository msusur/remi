using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.Reports;
using ReMi.Queries.Reports;
using ReMi.QueryHandlers.Reports;

namespace ReMi.QueryHandlers.Tests.Reports
{
    public class ReportHandlerTests : TestClassFor<ReportHandler>
    {
        private Mock<IReportGateway> _reportGatewayMock;

        protected override ReportHandler ConstructSystemUnderTest()
        {
            return new ReportHandler
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
        public void Handle_ShouldCallGatewayWithCorrectParameters_WhenInvoked()
        {
            var request = new ReportRequest
            {
                ReportName = "azaza",
                Parameters = new Dictionary<string, String> {{"a", "aaa"}, {"b", "2014.10.10 00:00:00"}}
            };
            var date = new DateTime(2014, 10, 10, 0, 0, 0).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");

            _reportGatewayMock.Setup(
                x =>
                    x.GetReport("azaza",
                        It.Is<IDictionary<string, object>>(
                            d =>
                                d.Any(pair => pair.Key.Equals("a") && pair.Value.Equals("aaa")) &&
                                d.Any(pair => pair.Key.Equals("b") && pair.Value.Equals(date))
                            )));

            Sut.Handle(request);

            _reportGatewayMock.Verify(
                x =>
                    x.GetReport("azaza",
                        It.Is<IDictionary<string, object>>(
                            d =>
                                d.Any(pair => pair.Key.Equals("a") && pair.Value.Equals("aaa")) &&
                                d.Any(pair => pair.Key.Equals("b") && pair.Value.Equals(date))
                            )));
        }

        [Test]
        public void Handle_ShouldCallGateway_WhenInvokedWithoutParameters()
        {
            var request = new ReportRequest
            {
                ReportName = "azaza"
            };
           
          Sut.Handle(request);

            _reportGatewayMock.Verify(
                x =>
                    x.GetReport("azaza",
                        It.Is<IDictionary<string, object>>(
                            d =>
                                d.Count == 0
                            )));
        }
    }
}
