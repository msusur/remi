using System;
using NUnit.Framework;
using ReMi.TestUtils.UnitTests;
using ReMi.Queries.Auth;
using ReMi.QueryValidators.Auth;

namespace ReMi.QueryValidators.Tests.Auth
{
    public class PermissionsRequestValidatorTests : TestClassFor<PermissionsRequestValidator>
    {
        protected override PermissionsRequestValidator ConstructSystemUnderTest()
        {
            return new PermissionsRequestValidator();
        }

        [Test]
        public void Validate_ShouldReturnNoErrors_WhenRoleIdIsCorrect()
        {
            var result = Sut.Validate(new PermissionsRequest
            {
                RoleId = Guid.NewGuid()
            });

            Assert.AreEqual(0, result.Errors.Count);
        }

        [Test]
        public void Validate_ShouldReturnOneError_WhenRoleIdIsNotCorrect()
        {
            var result = Sut.Validate(new PermissionsRequest());

            Assert.AreEqual(1, result.Errors.Count);
        }
    }
}
