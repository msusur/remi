using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ReMi.TestUtils.UnitTests;
using ReMi.Queries.BusinessRules;
using ReMi.QueryValidators.BusinessRules;

namespace ReMi.QueryValidators.Tests.BusinessRules
{
    public class TriggerBusinessRuleValidatorTests : TestClassFor<TriggerBusinessRuleValidator>
    {
        protected override TriggerBusinessRuleValidator ConstructSystemUnderTest()
        {
            return new TriggerBusinessRuleValidator();
        }

        [Test]
        public void Validate_ShouldNotPass_WhenRequestIsEmpty()
        {
            var query = new TriggerBusinessRuleRequest();

            var result = Sut.Validate(query);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Rule or ExternalId", result.Errors.ElementAt(0).PropertyName);
        }

        [Test]
        public void Validate_ShouldNotPass_WhenParameterHasEmptyBody()
        {
            var query = new TriggerBusinessRuleRequest
            {
                ExternalId = Guid.NewGuid(),
                Parameters = new Dictionary<string, string>
                {
                    { RandomData.RandomString(10), RandomData.RandomString(10) },
                    { RandomData.RandomString(10), string.Empty }
                }
            };

            var result = Sut.Validate(query);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Parameters[1].Value", result.Errors.ElementAt(0).PropertyName);
        }

        [Test]
        public void Validate_ShouldPass_WhenRuleIdIsNotEmpty()
        {
            var query = new TriggerBusinessRuleRequest
            {
                ExternalId = Guid.NewGuid()
            };

            var result = Sut.Validate(query);

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void Validate_ShouldPass_WhenNameIsNotEmpty()
        {
            var query = new TriggerBusinessRuleRequest
            {
                Rule = RandomData.RandomString(10)
            };

            var result = Sut.Validate(query);

            Assert.IsTrue(result.IsValid);
        }
    }
}
