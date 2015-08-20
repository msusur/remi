using System;
using NUnit.Framework;
using ReMi.TestUtils.UnitTests;
using ReMi.Queries.Plugins;
using ReMi.QueryValidators.Plugins;

namespace ReMi.QueryValidators.Tests.Plugins
{
    public class GetPluginsConfigurationRequestValidatorTests : TestClassFor<GetPluginsConfigurationRequestValidator>
    {
        protected override GetPluginsConfigurationRequestValidator ConstructSystemUnderTest()
        {
            return new GetPluginsConfigurationRequestValidator();
        }

        [Test]
        public void Validate_ShouldReturnError_WhenRequestIsEmpty()
        {
            var request = new GetPluginsConfigurationRequest();

            var result = Sut.Validate(request);

            Assert.AreEqual(1, result.Errors.Count, "error list size");
            Assert.True(result.Errors[0].PropertyName.Contains("PluginId"));
        }

        [Test]
        public void Validate_ShouldNotReturnErrors_WhenRequestIsOk()
        {
            var request = new GetPluginsConfigurationRequest
            {
                PluginId = Guid.NewGuid()
            };

            var result = Sut.Validate(request);

            Assert.AreEqual(0, result.Errors.Count, "error list size");
        }
    }
}
