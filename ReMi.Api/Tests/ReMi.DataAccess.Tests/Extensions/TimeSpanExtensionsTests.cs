using System;
using NUnit.Framework;
using ReMi.Common.Utils;

namespace ReMi.DataAccess.Tests.Extensions
{
    [TestFixture]
    public class TimeSpanExtensionsTests
    {
        [TestFixtureSetUp]
        public void SetUp()
        {

        }

        [Test]
        public void ToDurationString_ShouldReturnEmptyString_WhenTimespanEmpty()
        {
            var ts = new TimeSpan();

            var result = ts.ToDurationString();

            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        [TestCase(0, 0, 0, 1, 0, "1 s.")]
        [TestCase(0, 0, 1, 0, 0, "1 min.")]
        [TestCase(0, 1, 0, 0, 0, "1 h.")]
        [TestCase(1, 0, 0, 0, 0, "1 d.")]
        [TestCase(0, 0, 0, 62, 0, "1 min. 2 s.")]
        [TestCase(0, 0, 0, 62, 14, "1 min. 2 s.")]
        [TestCase(0, 0, 0, 3664, 0, "1 h. 1 min. 4 s.")]
        [TestCase(0, 0, 0, 0, 20000, "20 s.")]
        [TestCase(0, 0, 0, 0, 20400, "20 s.")]
        public void ToDurationString_ShouldReturnCorrectString_WhenTimespanEmpty(int days, int hours, int minutes, int seconds, int mseconds, string expected)
        {
            var ts = new TimeSpan(days, hours, minutes, seconds, mseconds);

            var result = ts.ToDurationString();

            Assert.AreEqual(expected, result);
        }
    }
}
