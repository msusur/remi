using NUnit.Framework;
using ReMi.DataAccess.Helpers;
using System.Linq;

namespace ReMi.DataAccess.Tests.Helpers
{
    [TestFixture]
    public class QueryCollectorTests
    {
        [Test]
        public void Collect_ShouldNotBeEmptyResult_WhenInvoked()
        {
            var result = QueryCollector.Collect();

            Assert.IsNotEmpty(result);
            Assert.IsTrue(result.All(x => !string.IsNullOrEmpty(x.Description)), "Some query 'Description' is null or empty");
            Assert.IsTrue(result.All(x => !string.IsNullOrEmpty(x.Name)), "Some query 'Name' is null or empty");
            Assert.IsTrue(result.All(x => !string.IsNullOrEmpty(x.Group)), "Some query 'Group' is null or empty");
            Assert.IsTrue(result.Any(x => x.IsStatic), "All queries are not statis, something is wrong probably");
            Assert.IsTrue(result.All(x => !string.IsNullOrEmpty(x.Namespace)), "Some query 'Namespace' is null or empty");
        }
    }
}
