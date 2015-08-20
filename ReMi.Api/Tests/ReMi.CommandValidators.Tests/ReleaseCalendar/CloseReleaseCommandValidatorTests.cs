using System.Collections.Generic;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.Commands.ReleaseCalendar;
using ReMi.CommandValidators.ReleaseCalendar;
using System;
using System.Linq;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandValidators.Tests.ReleaseCalendar
{
    [TestFixture]
    public class CloseReleaseCommandValidatorTests : TestClassFor<CloseReleaseCommandValidator>
    {
        protected override CloseReleaseCommandValidator ConstructSystemUnderTest()
        {
            return new CloseReleaseCommandValidator();
        }

        [Test]
        public void Validate_ShouldPass_WhenCommandValid()
        {
            var command = new CloseReleaseCommand
            {
                ReleaseWindowId = Guid.NewGuid(),
                Recipients = new List<Account>
                {
                    new Account { ExternalId = Guid.NewGuid(), Email = RandomData.RandomEmail() },
                    new Account { ExternalId = Guid.NewGuid(), Email = RandomData.RandomEmail() }
                },
                UserName = RandomData.RandomEmail(),
                Password = RandomData.RandomString(10)
            };

            var result = Sut.Validate(command);

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void Validate_ShouldNotPass_WhenPropertiesAreEmpty()
        {
            var command = new CloseReleaseCommand
            {
                ReleaseWindowId = Guid.Empty,
                Recipients = new List<Account>
                {
                    new Account { ExternalId = Guid.NewGuid(), Email = RandomData.RandomString(10) },
                    new Account { ExternalId = Guid.NewGuid() },
                    new Account { Email = RandomData.RandomEmail() }
                },
                UserName = string.Empty,
                Password = string.Empty
            };


            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(6, result.Errors.Count);
            Assert.AreEqual("ReleaseWindowId", result.Errors.ElementAt(0).PropertyName);
            Assert.AreEqual("Recipients[0].Email", result.Errors.ElementAt(1).PropertyName);
            Assert.AreEqual("Recipients[1].Email", result.Errors.ElementAt(2).PropertyName);
            Assert.AreEqual("Recipients[2].ExternalId", result.Errors.ElementAt(3).PropertyName);
            Assert.AreEqual("UserName", result.Errors.ElementAt(4).PropertyName);
            Assert.AreEqual("Password", result.Errors.ElementAt(5).PropertyName);
        }
    }
}
