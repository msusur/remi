using System;
using System.Linq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ProductRequests;
using ReMi.CommandValidators.Common;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandValidators.Tests.Common
{
    [TestFixture]
    public class ProductRequestAssigneesValidatorTests : TestClassFor<ProductRequestAssigneesValidator>
    {
        protected override ProductRequestAssigneesValidator ConstructSystemUnderTest()
        {
            return new ProductRequestAssigneesValidator();
        }

        [Test]
        public void Validate_ShouldPass_WhenAccountsHaveValidCodes()
        {
            var accounts = new[]
            {
                new Account { ExternalId = Guid.NewGuid()},
                new Account { ExternalId = Guid.NewGuid()}
            };

            var result = Sut.Validate(new ProductRequestGroup { Assignees = accounts });

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void Validate_ShouldPass_WhenAssigneesEmpty()
        {
            var accounts = new Account[0];

            var result = Sut.Validate(new ProductRequestGroup { Assignees = accounts });

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void Validate_ShouldNotPass_WhenAssigneesHasEmptyExternalId()
        {
            var accounts = new[]
            {
                new Account { ExternalId = Guid.Empty},
                new Account { ExternalId = Guid.NewGuid()}
            };

            var result = Sut.Validate(new ProductRequestGroup { Assignees = accounts });

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Assignees[0].ExternalId", result.Errors.ElementAt(0).PropertyName);
        }
    }
}
