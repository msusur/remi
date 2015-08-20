using System.Linq;
using NUnit.Framework;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Common.Constants;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Utils.Enums;
using ReMi.TestUtils.UnitTests;
using ReMi.Queries.Configuration;
using ReMi.QueryHandlers.Configuration;

namespace ReMi.QueryHandlers.Tests.Configuration
{
    public class GetReleaseTrackHandlerTests : TestClassFor<GetReleaseTracksHandler>
    {
        protected override GetReleaseTracksHandler ConstructSystemUnderTest()
        {
            return new GetReleaseTracksHandler();
        }

        [Test]
        public void Handle_ShouldReturnReleaseTrack()
        {
            var items = EnumDescriptionHelper.GetEnumDescriptions<ReleaseTrack, ReleaseTrackDescription>();

            var result = Sut.Handle(new GetReleaseTrackRequest());

            Assert.AreEqual(items.Length, result.ReleaseTrack.Count(), "response size");
        }
    }
}
