using NUnit.Framework;
using ReMi.TestUtils.UnitTests;
using ReMi.Queries.Reports;
using ReMi.QueryValidators.Reports;

namespace ReMi.QueryValidators.Tests.Reports
{
    public class ReportRequestValidatorTests : TestClassFor<ReportRequestValidator>
    {
        protected override ReportRequestValidator ConstructSystemUnderTest()
        {
            return new ReportRequestValidator();
        }

        [Test]
        public void Validate_ShouldReturnError_WhenReportNameIsEmpty()
        {
            var result = Sut.Validate(new ReportRequest());

            Assert.AreEqual(1, result.Errors.Count);
        }

        [Test]
        public void Validate_ShouldNotReturnError_WhenReportNameIsCorrect()
        {
            var result = Sut.Validate(new ReportRequest
            {
                ReportName = "azaza"
            });

            Assert.AreEqual(0, result.Errors.Count);
        }
    }
}
