using NUnit.Framework;
using ReMi.TestUtils.UnitTests;
using ReMi.Queries.ReleaseCalendar;
using ReMi.QueryValidators.ReleaseCalendar;

namespace ReMi.QueryValidators.Tests.ReleaseCalendar
{
    public class GetCurrentReleaseRequestValidatorTests : TestClassFor<GetCurrentReleaseRequestValidator>
    {
        protected override GetCurrentReleaseRequestValidator ConstructSystemUnderTest()
        {
            return new GetCurrentReleaseRequestValidator();
        }

        [Test]
        public void Validate_ShouldBeInvalid_WhenProductIsNull()
        {
            var request = new GetCurrentReleaseRequest
            {
                Product = null
            };

            var result = Sut.Validate(request);

            Assert.AreEqual(1, result.Errors.Count, "error list size");
            Assert.True(result.Errors[0].PropertyName.Contains("Product"));
        }

        [Test]
        public void Validate_ShouldBeInvalid_WhenProductIsEmpty()
        {
            var request = new GetCurrentReleaseRequest
            {
                Product = string.Empty
            };

            var result = Sut.Validate(request);

            Assert.AreEqual(1, result.Errors.Count, "error list size");
            Assert.True(result.Errors[0].PropertyName.Contains("Product"));
        }

        [Test]
        public void Validate_ShouldBeValid_WhenRequestIsCorrect()
        {
            var request = new GetCurrentReleaseRequest
            {
                Product = RandomData.RandomString(10)
            };

            var result = Sut.Validate(request);

            Assert.AreEqual(0, result.Errors.Count, "error list size");
        }
    }
}
