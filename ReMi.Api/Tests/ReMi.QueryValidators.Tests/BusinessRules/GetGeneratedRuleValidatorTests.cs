using NUnit.Framework;
using ReMi.Queries.BusinessRules;
using ReMi.QueryValidators.BusinessRules;
using System.Linq;
using ReMi.TestUtils.UnitTests;

namespace ReMi.QueryValidators.Tests.BusinessRules
{
    public class GetGeneratedRuleValidatorTests : TestClassFor<GetGeneratedRuleValidator>
    {
        protected override GetGeneratedRuleValidator ConstructSystemUnderTest()
        {
            return new GetGeneratedRuleValidator();
        }

        [Test]
        public void Validate_ShouldNotPass_WhenRequestIsEmpty()
        {
            var query = new GetGeneratedRuleRequest();

            var result = Sut.Validate(query);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(2, result.Errors.Count);
            Assert.AreEqual("Name", result.Errors.ElementAt(0).PropertyName);
            Assert.AreEqual("Namespace", result.Errors.ElementAt(1).PropertyName);
        }

        [Test]
        public void Validate_ShouldPass_WhenRequestHasNameAndNamespace()
        {
            var query = new GetGeneratedRuleRequest
            {
                Name = RandomData.RandomString(10),
                Namespace = RandomData.RandomString(10)
            };

            var result = Sut.Validate(query);

            Assert.IsTrue(result.IsValid);
        }
    }
}
