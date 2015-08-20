using System;
using System.Linq;
using NUnit.Framework;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.TestUtils.UnitTests;
using ReMi.Queries.ReleaseCalendar;
using ReMi.QueryHandlers.ReleaseCalendar;

namespace ReMi.QueryHandlers.Tests.ReleaseCalendar
{
    public class GetReleaseTypesHandlerTests : TestClassFor<GetReleaseEnumsHandler>
    {
        protected override GetReleaseEnumsHandler ConstructSystemUnderTest()
        {
            return new GetReleaseEnumsHandler();
        }

        [Test]
        public void Handle_ShouldGetAllReleaseTypes_WhenInvoked()
        {
            var expectedTypesCount = Enum.GetValues(typeof(ReleaseType)).Length;
            var request = new GetReleaseEnumsRequest();

            var result = Sut.Handle(request);

            Assert.AreEqual(expectedTypesCount, result.ReleaseTypes.Count());
            Assert.IsTrue(result.ReleaseTypes.All(x => !string.IsNullOrEmpty(x.Name)));
            Assert.IsTrue(result.ReleaseTypes.All(x => !string.IsNullOrEmpty(x.Description)));
        }
    }
}
