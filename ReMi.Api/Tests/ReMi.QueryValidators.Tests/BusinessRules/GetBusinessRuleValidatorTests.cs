using System;
using System.Linq;
using NUnit.Framework;
using ReMi.TestUtils.UnitTests;
using ReMi.Queries.BusinessRules;
using ReMi.QueryValidators.BusinessRules;

namespace ReMi.QueryValidators.Tests.BusinessRules
{
    public class GetBusinessRuleValidatorTests : TestClassFor<GetBusinessRuleValidator>
    {
        protected override GetBusinessRuleValidator ConstructSystemUnderTest()
        {
            return new GetBusinessRuleValidator();
        }

        [Test]
        public void Validate_ShouldNotPass_WhenRequestIsEmpty()
        {
            var query = new GetBusinessRuleRequest();

            var result = Sut.Validate(query);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Name or ExternalId", result.Errors.ElementAt(0).PropertyName);
        }

        [Test]
        public void Validate_ShouldPass_WhenRequestHasNotEmptyExternalId()
        {
            var query = new GetBusinessRuleRequest
            {
                ExternalId = Guid.NewGuid()
            };

            var result = Sut.Validate(query);

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void Validate_ShouldPass_WhenRequestHasNotEmptyName()
        {
            var query = new GetBusinessRuleRequest
            {
                Name = RandomData.RandomString(10)
            };

            var result = Sut.Validate(query);

            Assert.IsTrue(result.IsValid);
        }
    }
}
