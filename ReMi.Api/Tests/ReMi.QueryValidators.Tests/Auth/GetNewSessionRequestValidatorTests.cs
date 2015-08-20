using System;
using NUnit.Framework;
using ReMi.TestUtils.UnitTests;
using ReMi.Queries.Auth;
using ReMi.QueryValidators.Auth;

namespace ReMi.QueryValidators.Tests.Auth
{
    public class GetNewSessionRequestValidatorTests : TestClassFor<GetNewSessionRequestValidator>
    {
        protected override GetNewSessionRequestValidator ConstructSystemUnderTest()
        {
            return new GetNewSessionRequestValidator();
        }

        [Test]
        public void Validate_ShouldReturnNoErrors_WhenRoleIdIsCorrect()
        {
            var result = Sut.Validate(new GetNewSessionRequest
            {
                SessionId = Guid.NewGuid()
            });

            Assert.AreEqual(0, result.Errors.Count);
        }

        [Test]
        public void Validate_ShouldReturnOneError_WhenRoleIdIsNotCorrect()
        {
            var result = Sut.Validate(new GetNewSessionRequest());

            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("'Session Id' should not be empty.", result.Errors[0].ErrorMessage);
        }
    }
}
