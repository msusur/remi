using System;
using NUnit.Framework;
using ReMi.BusinessEntities.Api;
using ReMi.Commands;
using ReMi.CommandValidators.Api;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandValidators.Tests.Api
{
    public class UpdateApiDescriptionCommandValidatorTests : TestClassFor<UpdateApiDescriptionCommandValidator>
    {
        protected override UpdateApiDescriptionCommandValidator ConstructSystemUnderTest()
        {
            return new UpdateApiDescriptionCommandValidator();
        }

        [Test]
        [TestCase("GET")]
        [TestCase("POST")]
        public void Validate_ShouldReturnValidResult_WhenDataIsCorrect(String method)
        {
            var command = new UpdateApiDescriptionCommand
            {
                ApiDescription = new ApiDescription
                {
                    Method = method,
                    Url = "smth"
                }
            };

            var result = Sut.Validate(command);

            Assert.True(result.IsValid);
        }

        [Test]
        public void Validate_ShouldReturnNotValidResult_WhenMethodIsNotCorrect()
        {
            var command = new UpdateApiDescriptionCommand
            {
                ApiDescription = new ApiDescription
                {
                    Method = "azaz",
                    Url = "smth"
                }
            };

            var result = Sut.Validate(command);

            Assert.False(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count, "error list size");
            Assert.AreEqual("Incorrect HTTP method in API description", result.Errors[0].ErrorMessage, "http method");
        }

        [Test]
        public void Validate_ShouldReturnNotValidResult_WhenUrlIsEmpty()
        {
            var command = new UpdateApiDescriptionCommand
            {
                ApiDescription = new ApiDescription
                {
                    Method = "post"
                }
            };

            var result = Sut.Validate(command);

            Assert.False(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count, "error list size");
            Assert.AreEqual("API description URL should not be empty", result.Errors[0].ErrorMessage, "url");
        }
    }
}
