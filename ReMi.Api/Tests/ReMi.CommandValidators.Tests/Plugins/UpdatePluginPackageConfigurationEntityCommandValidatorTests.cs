using NUnit.Framework;
using ReMi.Commands.Plugins;
using ReMi.CommandValidators.Plugins;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandValidators.Tests.Plugins
{
    public class UpdatePluginPackageConfigurationEntityCommandValidatorTests : TestClassFor<UpdatePluginPackageConfigurationEntityCommandValidator>
    {
        protected override UpdatePluginPackageConfigurationEntityCommandValidator ConstructSystemUnderTest()
        {
            return new UpdatePluginPackageConfigurationEntityCommandValidator();
        }

        [Test]
        public void Validate_ShouldReturn3Errors_WhenCommandIsEmpty()
        {
            var command = new UpdatePluginPackageConfigurationEntityCommand();

            var result = Sut.Validate(command);

            Assert.AreEqual(3, result.Errors.Count, "error list size");
            Assert.True(result.Errors[0].PropertyName.Contains("PluginKey"));
            Assert.True(result.Errors[1].PropertyName.Contains("PackageName"));
            Assert.True(result.Errors[2].PropertyName.Contains("PropertyName"));
        }

        [Test]
        public void Validate_ShouldReturnError_WhenPluginIdIsEmpytButNotNull()
        {
            var command = new UpdatePluginPackageConfigurationEntityCommand
            {
                PluginKey = string.Empty,
                PackageName = string.Empty,
                JsonValue = string.Empty,
                PropertyName = string.Empty
            };

            var result = Sut.Validate(command);

            Assert.AreEqual(3, result.Errors.Count, "error list size");
            Assert.True(result.Errors[0].PropertyName.Contains("PluginKey"));
            Assert.True(result.Errors[1].PropertyName.Contains("PackageName"));
            Assert.True(result.Errors[2].PropertyName.Contains("PropertyName"));
        }

        [Test]
        public void Validate_ShouldNotReturnErrors_WhenCommandIsOk()
        {
            var command = new UpdatePluginPackageConfigurationEntityCommand
            {
                PluginKey = RandomData.RandomString(10),
                PackageName = RandomData.RandomString(10),
                PropertyName = RandomData.RandomString(10)
            };

            var result = Sut.Validate(command);

            Assert.AreEqual(0, result.Errors.Count, "error list size");
        }
    }
}
