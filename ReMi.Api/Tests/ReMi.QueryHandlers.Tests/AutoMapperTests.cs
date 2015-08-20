using AutoMapper;
using FizzWare.NBuilder;
using NUnit.Framework;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Common.Utils;
using ReMi.Queries.ReleaseCalendar;
using ReMi.QueryHandlers.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.TestUtils.UnitTests;

namespace ReMi.QueryHandlers.Tests
{
    [TestFixture]
    public class AutoMapperTests : TestClassFor<IMappingEngine>
    {
        [TestFixtureSetUp]
        public static void AutoMapperInitialize()
        {
            Mapper.Initialize(
                c =>
                {
                    c.AddProfile<ApiQueryToBusinessEntityMappingProfile>();
                });

        }

        protected override IMappingEngine ConstructSystemUnderTest()
        {
            return Mapper.Engine;
        }

        [Test]
        public void Map_ShouldReturnReleaseCalendarFilter_WhenGetReleaseCalendarRequest()
        {
            var input = new GetReleaseCalendarRequest
            {
                StartDay = SystemTime.Now.Date.AddDays(-2), 
                EndDay = SystemTime.Now.Date
            };

            var result = Sut.Map<GetReleaseCalendarRequest, ReleaseCalendarFilter>(input);

            Assert.AreEqual(input.StartDay.ToUniversalTime(), result.StartDay);
            Assert.AreEqual(input.EndDay.ToUniversalTime(), result.EndDay);
            Assert.AreEqual(DateTimeKind.Utc, result.StartDay.Kind);
            Assert.AreEqual(DateTimeKind.Utc, result.EndDay.Kind);
        }
    }
}
