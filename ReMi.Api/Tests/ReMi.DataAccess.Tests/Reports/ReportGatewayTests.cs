using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ReMi.Common.Utils.Repository;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.Reports;
using ReMi.DataAccess.Exceptions;
using ReMi.DataAccess.Helpers;
using ReMi.DataEntities.Reports;

namespace ReMi.DataAccess.Tests.Reports
{
    public class ReportGatewayTests : TestClassFor<ReportGateway>
    {
        private Mock<IRepository<ReportDescription>> _reportDescriptionRepositoryMock;
        private Mock<IDatabaseAdapter> _databaseMock;

        protected override ReportGateway ConstructSystemUnderTest()
        {
            return new ReportGateway
            {
                ReportDesriptionRepository = _reportDescriptionRepositoryMock.Object,
                DatabaseAdapter = _databaseMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _reportDescriptionRepositoryMock = new Mock<IRepository<ReportDescription>>();
            _databaseMock = new Mock<IDatabaseAdapter>();

            base.TestInitialize();
        }

        [Test, ExpectedException(typeof (EntityNotFoundException))]
        public void GetReport_ShouldThrowException_WhenReportDescriptionWasNotFound()
        {
            Sut.GetReport(String.Empty, new Dictionary<string, object>());
        }

        [Test, ExpectedException(typeof (EntityNotFoundException))]
        public void GetReport_ShouldThrowException_WhenReportColumnsWereNotFound()
        {
            _reportDescriptionRepositoryMock.SetupEntities(new List<ReportDescription>
            {
                new ReportDescription {ProcedureName = "a"}
            });

            Sut.GetReport("a", new Dictionary<string, object>());
        }

        [Test]
        public void GetReportDescriptions_ShouldReturnCorrectData_WhenCalled()
        {
            _reportDescriptionRepositoryMock.SetupEntities(new List<ReportDescription>
            {
                new ReportDescription
                {
                    ProcedureName = "a",
                    Name = "b",
                    ReportParameters = new List<ReportParameter>
                    {
                        new ReportParameter
                        {
                            Name = "p",
                            Description = "P",
                            Type = "t"
                        }
                    }
                }
            });

            var result = Sut.GetReportDescriptions().ToList();

            Assert.AreEqual(1, result.Count, "report list size");
            Assert.AreEqual("b", result[0].ReportName, "report name");
            Assert.AreEqual("a", result[0].ReportCreator, "procedure name");
            Assert.AreEqual(1, result[0].ReportParameters.Count, "parameters size");
            Assert.AreEqual("p", result[0].ReportParameters[0].Name, "parameter name");
            Assert.AreEqual("P", result[0].ReportParameters[0].Description, "parameter description");
            Assert.AreEqual("t", result[0].ReportParameters[0].Type, "parameter type");
        }

        [Test]
        public void GetReport_ShouldReturnCorrectData_WhenParametersAreCorrect()
        {
            _reportDescriptionRepositoryMock.SetupEntities(new List<ReportDescription>
            {
                new ReportDescription
                {
                    ProcedureName = "a",
                    Name = "b",
                    ReportParameters = new List<ReportParameter>
                    {
                        new ReportParameter
                        {
                            Name = "p",
                            Description = "P",
                            Type = "t"
                        }
                    },
                    ReportColumns = new List<ReportColumn>
                    {
                        new ReportColumn
                        {
                            Name = "second",
                            Order = 2
                        },
                        new ReportColumn
                        {
                            Name = "first",
                            Order = 1
                        }
                    }
                }
            });

            _databaseMock.Setup(d => d.RunStoredProcedure("Report.a", 2, It.IsAny<IDictionary<string, object>>()))
                .Returns(new List<List<string>>
                {
                    new List<string> {"smth", "azaza"},
                    new List<string> {"other", "zazaz"}
                });

            var result = Sut.GetReport("a", new Dictionary<string, object> {{"p", "value"}});

            _databaseMock.Verify(d => d.RunStoredProcedure("Report.a", 2, It.IsAny<IDictionary<string, object>>()));
            Assert.AreEqual(2, result.Data.Count, "result size");
            Assert.AreEqual("first", result.Headers[0], "first column name");
            Assert.AreEqual("second", result.Headers[1], "second column name");
            Assert.AreEqual("smth", result.Data[0][0], "first column value");
            Assert.AreEqual("azaza", result.Data[0][1], "second column value");
            Assert.AreEqual("other", result.Data[1][0], "first column value");
            Assert.AreEqual("zazaz", result.Data[1][1], "second column value");

        }
    }
}
