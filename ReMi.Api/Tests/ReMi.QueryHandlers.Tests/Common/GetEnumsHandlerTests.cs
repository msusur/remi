using NUnit.Framework;
using ReMi.TestUtils.UnitTests;
using ReMi.Queries.Common;
using ReMi.QueryHandlers.Common;

namespace ReMi.QueryHandlers.Tests.Common
{
    public class GetEnumsHandlerTests : TestClassFor<GetEnumsHandler>
    {
        protected override GetEnumsHandler ConstructSystemUnderTest()
        {
            return new GetEnumsHandler();
        }

        [Test]
        [TestCase("RemovingReason", TestName = "GetEnums_ShouldGetRemovingReleaseDescriptions_WhenCalled")]
        [TestCase("ReleaseType", TestName = "GetEnums_ShouldGetReleaseTypeDescriptions_WhenCalled")]
        [TestCase("ReleaseTrack", TestName = "GetEnums_ShouldGetReleaseTrackDescriptions_WhenCalled")]
        [TestCase("ReleaseTaskEnvironment", TestName = "GetEnums_ShouldGetReleaseTaskEnvironmentDescriptions_WhenCalled")]
        [TestCase("ReleaseTaskRisk", TestName = "GetEnums_ShouldGetReleaseTaskRiskDescriptions_WhenCalled")]
        [TestCase("ReleaseTaskType", TestName = "GetEnums_ShouldGetReleaseTaskTypeDescriptions_WhenCalled")]
        [TestCase("PluginType", TestName = "GetEnums_ShouldGetPluginTypeDescriptions_WhenCalled")]
        [TestCase("JobStage", TestName = "GetEnums_ShouldGetJobStageDescriptions_WhenCalled")]
        public void GetEnums_ShouldGetEnumByName_WhenCalled(string enumName)
        {
            var result = Sut.Handle(new GetEnumsRequest());

            Assert.IsTrue(result.Enums.ContainsKey(enumName));
        }
    }
}
