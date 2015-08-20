using System;
using NUnit.Framework;
using ReMi.Common.Utils;

namespace ReMi.Api.Tests.Common
{
    [TestFixture]
    public class DateTimeExtensionsTests
    {
        [Test]
        public void ToMssqlRangeDateTimeNullable_ShouldReturnNull_IfDateIsNull()
        {
            DateTime? d = null;

            var result = d.ToMssqlRangeDateTime();

            Assert.IsNull(result);
        }

        [Test]
        public void ToMssqlRangeDateTimeNullable_ShouldReturnNull_IfDateOutOfValidRange()
        {
            DateTime? d = DateTime.MinValue;

            var result = d.ToMssqlRangeDateTime();

            Assert.IsNull(result);
        }

        [Test]
        public void ToMssqlRangeDateTimNullablee_ShouldReturnDate_IfDateIsValidMssqlDate()
        {
            DateTime? d = new DateTime(1753, 1, 1);

            var result = d.ToMssqlRangeDateTime();

            Assert.AreEqual(d, result);
        }

        [Test]
        public void ToMssqlRangeDateTime_ShouldReturnDefaultValue_IfDateOutOfValidRange()
        {
            var d = DateTime.MinValue;

            var result = d.ToMssqlRangeDateTime(DateTime.MaxValue);

            Assert.AreEqual(DateTime.MaxValue, result);
        }

        [Test]
        public void ToMssqlRangeDateTime_ShouldReturnDate_IfDateIsValidMssqlDate()
        {
            var d = new DateTime(1753, 1, 1);

            var result = d.ToMssqlRangeDateTime(DateTime.MaxValue);

            Assert.AreEqual(d, result);
        }
    }
}
