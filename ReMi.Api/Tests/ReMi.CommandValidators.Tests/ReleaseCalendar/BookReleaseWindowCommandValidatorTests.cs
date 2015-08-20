using System;
using System.Linq;
using NUnit.Framework;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Commands.ReleaseCalendar;
using ReMi.CommandValidators.ReleaseCalendar;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Utils;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandValidators.Tests.ReleaseCalendar
{
    public class BookReleaseWindowCommandValidatorTests : TestClassFor<BookReleaseWindowCommandValidator>
    {
        protected override BookReleaseWindowCommandValidator ConstructSystemUnderTest()
        {
            return new BookReleaseWindowCommandValidator();
        }

        [Test]
        public void Validate_ShouldPass_WhenCommandValid()
        {
            var command = new BookReleaseWindowCommand
            {
                ReleaseWindow = PrepareReleaseWindow()
            };

            var result = Sut.Validate(command);

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void Validate_ShouldFail_WhenProductsEmpty()
        {
            var releaseWindow = PrepareReleaseWindow();
            releaseWindow.Products = new[] { string.Empty };

            var command = new BookReleaseWindowCommand
            {
                ReleaseWindow = releaseWindow
            };

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
        }

        [Test]
        public void Validate_ShouldFail_WhenSprintEmpty()
        {
            var releaseWindow = PrepareReleaseWindow();
            releaseWindow.Sprint = string.Empty;

            var command = new BookReleaseWindowCommand
            {
                ReleaseWindow = releaseWindow
            };

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(2, result.Errors.Count);

            Assert.IsTrue(result.Errors.Any(x => x.ErrorMessage == "'Release Window. Sprint' should not be empty."));
            Assert.IsTrue(result.Errors.Any(x => x.ErrorMessage == "'Release Window. Sprint' must be between 1 and 128 characters. You entered 0 characters."));
        }

        [Test]
        public void Validate_ShouldFail_WhenSprintTooLong()
        {
            var releaseWindow = PrepareReleaseWindow();
            releaseWindow.Sprint = new string('*', 129);

            var command = new BookReleaseWindowCommand
            {
                ReleaseWindow = releaseWindow
            };

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);

            Assert.IsTrue(result.Errors.Any(x => x.ErrorMessage == "'Release Window. Sprint' must be between 1 and 128 characters. You entered 129 characters."));
        }

        [Test]
        public void Validate_ShouldFail_WhenDescriptionTooLong()
        {
            var releaseWindow = PrepareReleaseWindow();
            releaseWindow.Description = new string('*', 1025);

            var command = new BookReleaseWindowCommand
            {
                ReleaseWindow = releaseWindow
            };

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
        }

        [Test]
        public void Validate_ShouldFail_WhenExternalIdEmpty()
        {
            var releaseWindow = PrepareReleaseWindow();
            releaseWindow.ExternalId = Guid.Empty;

            var command = new BookReleaseWindowCommand
            {
                ReleaseWindow = releaseWindow
            };

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
        }

        [Test]
        public void Validate_ShouldFail_WhenStartTimeGraterThenEndTime()
        {
            var releaseWindow = PrepareReleaseWindow();
            releaseWindow.StartTime = DateTime.Now.AddHours(2);
            releaseWindow.EndTime = DateTime.Now.AddHours(1);

            var command = new BookReleaseWindowCommand
            {
                ReleaseWindow = releaseWindow
            };

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
        }

        [Test]
        public void Validate_ShouldFail_WhenStartTimeInPast()
        {
            SystemTime.Mock(DateTime.Now);

            var releaseWindow = PrepareReleaseWindow();
            releaseWindow.StartTime = DateTime.Now.AddDays(-1);

            var command = new BookReleaseWindowCommand
            {
                ReleaseWindow = releaseWindow
            };

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
        }

        [Test]
        public void Validate_ShouldFail_WhenHotfioxRequiresDowntime()
        {
            var releaseWindow = PrepareReleaseWindow();
            releaseWindow.ReleaseType = ReleaseType.Hotfix;
            releaseWindow.RequiresDowntime = true;

            var command = new BookReleaseWindowCommand
            {
                ReleaseWindow = releaseWindow
            };

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
        }

        [Test]
        public void Validate_ShouldFail_WhenReleaseTypeAutomated()
        {
            var releaseWindow = PrepareReleaseWindow();
            releaseWindow.ReleaseType = ReleaseType.Automated;

            var command = new BookReleaseWindowCommand
            {
                ReleaseWindow = releaseWindow
            };

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
        }

        private ReleaseWindow PrepareReleaseWindow()
        {
            return new ReleaseWindow
            {
                ExternalId = Guid.NewGuid(),
                Products = new[] { "P1" },
                Sprint = RandomData.RandomString(10),
                Description = RandomData.RandomString(10),
                StartTime = DateTime.Now.AddHours(1),
                EndTime = DateTime.Now.AddHours(2),
                ReleaseType = ReleaseType.Scheduled
            };
        }
    }
}
