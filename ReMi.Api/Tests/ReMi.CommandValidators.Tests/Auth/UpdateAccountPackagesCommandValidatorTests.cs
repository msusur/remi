using System;
using System.Linq;
using NUnit.Framework;
using ReMi.Commands.Auth;
using ReMi.CommandValidators.Auth;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandValidators.Tests.Auth
{
    public class UpdateAccountPackagesCommandValidatorTests : TestClassFor<UpdateAccountPackagesCommandValidator>
    {
        protected override UpdateAccountPackagesCommandValidator ConstructSystemUnderTest()
        {
            return new UpdateAccountPackagesCommandValidator();
        }

        [Test]
        public void Validate_ShouldBeValid_WhenAllRequiredFieldsAreValid()
        {
            var command = new UpdateAccountPackagesCommand
            {
                AccountId = Guid.NewGuid(),
                PackageIds = new[] { Guid.NewGuid() }
            };
            command.DefaultPackageId = command.PackageIds.ElementAt(0);

            var result = Sut.Validate(command);

            Assert.True(result.IsValid);
        }

        [Test]
        public void Validate_ShouldNotBeValid_WhenDefaultPackageIdIsNotOnPackageList()
        {
            var command = new UpdateAccountPackagesCommand
            {
                AccountId = Guid.NewGuid(),
                PackageIds = new[] { Guid.NewGuid() },
                DefaultPackageId = Guid.NewGuid()
            };

            var result = Sut.Validate(command);

            Assert.False(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("PackageIds", result.Errors[0].PropertyName);
        }

        [Test]
        public void Validate_ShouldNotBeValid_WhenOneOfPackageIdIsEmpty()
        {
            var command = new UpdateAccountPackagesCommand
            {
                AccountId = Guid.NewGuid(),
                PackageIds = new[] { Guid.NewGuid(), Guid.Empty }
            };
            command.DefaultPackageId = command.PackageIds.ElementAt(0);

            var result = Sut.Validate(command);

            Assert.False(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("PackageIds[1]", result.Errors[0].PropertyName);
        }

        [Test]
        public void Validate_ShouldNotBeValidAndNotThrowException_WhenPackageIdsListIsNull()
        {
            var command = new UpdateAccountPackagesCommand
            {
                AccountId = Guid.NewGuid(),
                DefaultPackageId = Guid.NewGuid()
            };

            var result = Sut.Validate(command);

            Assert.False(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("PackageIds", result.Errors[0].PropertyName);
        }

        [Test]
        public void Validate_ShouldNotBeValidAndNotThrowException_WhenPackageIdsListIsEmpty()
        {
            var command = new UpdateAccountPackagesCommand
            {
                AccountId = Guid.NewGuid(),
                PackageIds = Enumerable.Empty<Guid>(),
                DefaultPackageId = Guid.NewGuid()
            };

            var result = Sut.Validate(command);

            Assert.False(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("PackageIds", result.Errors[0].PropertyName);
        }

        [Test]
        public void Validate_ShouldNotBeValid_WhenDefaultPackageIdIsEmpty()
        {
            var command = new UpdateAccountPackagesCommand
            {
                AccountId = Guid.NewGuid(),
                PackageIds = new[] { Guid.NewGuid() },
                DefaultPackageId = Guid.Empty
            };

            var result = Sut.Validate(command);

            Assert.False(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("DefaultPackageId", result.Errors[0].PropertyName);
        }

        [Test]
        public void Validate_ShouldNotBeValid_WhenAccountIdIsEmpty()
        {
            var command = new UpdateAccountPackagesCommand
            {
                AccountId = Guid.Empty,
                PackageIds = new[] { Guid.NewGuid() },
            };
            command.DefaultPackageId = command.PackageIds.ElementAt(0);

            var result = Sut.Validate(command);

            Assert.False(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("AccountId", result.Errors[0].PropertyName);
        }
    }
}
