using NUnit.Framework;
using ReMi.Commands.ReleaseCalendar;
using ReMi.CommandValidators.ReleaseCalendar;
using System;
using System.Linq;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandValidators.Tests.ReleaseCalendar
{
    [TestFixture]
    public class ClearApproversSignaturesCommandValidatorTests : TestClassFor<ClearApproversSignaturesCommandValidator>
    {
        protected override ClearApproversSignaturesCommandValidator ConstructSystemUnderTest()
        {
            return new ClearApproversSignaturesCommandValidator();
        }

        [Test]
        public void Validate_ShouldPass_WhenCommandValid()
        {
            var command = new ClearApproversSignaturesCommand
            {
                ReleaseWindowId = Guid.NewGuid()
            };

            var result = Sut.Validate(command);

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void Validate_ShouldNotPass_WhenPropertiesAreEmpty()
        {
            var command = new ClearApproversSignaturesCommand
            {
                ReleaseWindowId = Guid.Empty
            };
            
            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("ReleaseWindowId", result.Errors.ElementAt(0).PropertyName);
        }
    }
}
