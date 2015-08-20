using NUnit.Framework;
using ReMi.DataAccess.Helpers;
using System.Linq;

namespace ReMi.DataAccess.Tests.Helpers
{
    [TestFixture]
    public class CommandCollectorTests
    {
        [Test]
        public void Collect_ShouldNotBeEmptyResult_WhenInvoked()
        {
            var result = CommandCollector.Collect();

            Assert.IsNotEmpty(result);
            Assert.IsTrue(result.All(x => !string.IsNullOrEmpty(x.Description)), "Some command 'Description' is null or empty");
            Assert.IsTrue(result.All(x => !string.IsNullOrEmpty(x.Name)), "Some command 'Name' is null or empty");
            Assert.IsTrue(result.All(x => !string.IsNullOrEmpty(x.Group)), "Some command 'Group' is null or empty");
            Assert.IsTrue(result.Any(x => x.IsBackground), "All commands are not background, something is wrong probably");
            Assert.IsTrue(result.All(x => !string.IsNullOrEmpty(x.Namespace)), "Some command 'Namespace' is null or empty");
        }
    }
}
