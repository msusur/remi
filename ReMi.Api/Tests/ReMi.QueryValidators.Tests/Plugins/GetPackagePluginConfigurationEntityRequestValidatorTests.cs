using System;
using NUnit.Framework;
using ReMi.TestUtils.UnitTests;
using ReMi.Queries.Plugins;
using ReMi.QueryValidators.Plugins;

namespace ReMi.QueryValidators.Tests.Plugins
{
    public class GetPackagePluginConfigurationEntityRequestValidatorTests : TestClassFor<GetPackagePluginConfigurationEntityRequestValidator>
    {
        protected override GetPackagePluginConfigurationEntityRequestValidator ConstructSystemUnderTest()
        {
            return new GetPackagePluginConfigurationEntityRequestValidator();
        }

        [Test]
        public void Validate_ShouldReturnError_WhenRequestIsEmpty()
        {
            var request = new GetPackagePluginConfigurationEntityRequest();

            var result = Sut.Validate(request);

            Assert.AreEqual(2, result.Errors.Count, "error list size");
            Assert.True(result.Errors[0].PropertyName.Contains("PluginId"));
            Assert.True(result.Errors[1].PropertyName.Contains("PackageId"));
        }

        [Test]
        public void Validate_ShouldNotReturnErrors_WhenRequestIsOk()
        {
            var request = new GetPackagePluginConfigurationEntityRequest
            {
                PluginId = Guid.NewGuid(),
                PackageId = Guid.NewGuid()
            };

            var result = Sut.Validate(request);

            Assert.AreEqual(0, result.Errors.Count, "error list size");
        }
    }
}
