using NUnit.Framework;
using BusinessReleaseWindow = ReMi.BusinessEntities.ReleaseCalendar.ReleaseWindow;

namespace ReMi.CommandHandlers.Tests
{
    public static class AssertBusinessEntities
    {
        public static void PropertiesAreSame(BusinessReleaseWindow expected, BusinessReleaseWindow actual)
        {
            Assert.AreEqual(expected.ExternalId, actual.ExternalId);
            Assert.AreEqual(expected.ReleaseType.ToString(), actual.ReleaseType.ToString());
            Assert.AreEqual(expected.RequiresDowntime, actual.RequiresDowntime);
            Assert.AreEqual(expected.StartTime, actual.StartTime);

            CollectionAssert.AreEquivalent(expected.Products, actual.Products);
        }
    }
}
