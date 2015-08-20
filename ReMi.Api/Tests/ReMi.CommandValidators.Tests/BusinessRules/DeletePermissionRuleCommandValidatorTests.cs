using System;
using System.Linq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ProductRequests;
using ReMi.Commands.BusinessRules;
using ReMi.CommandValidators.BusinessRules;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandValidators.Tests.BusinessRules
{
    public class DeletePermissionRuleCommandValidatorTests : TestClassFor<DeletePermissionRuleCommandValidator>
    {
        protected override DeletePermissionRuleCommandValidator ConstructSystemUnderTest()
        {
            return new DeletePermissionRuleCommandValidator();
        }

        [Test]
        public void Validate_ShouldNotPass_WhenRuleIdIsEmpty()
        {
            var command = new DeletePermissionRuleCommand();

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("RuleId", result.Errors.ElementAt(0).PropertyName);
        }

        [Test]
        public void Validate_ShouldPass_WhenRuleIdIsNotEmpty()
        {
            var command = new DeletePermissionRuleCommand
            {
                RuleId = Guid.NewGuid()
            };

            var result = Sut.Validate(command);

            Assert.IsTrue(result.IsValid);
        }
    }
}
