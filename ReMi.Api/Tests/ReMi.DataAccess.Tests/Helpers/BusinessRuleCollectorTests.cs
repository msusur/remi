using System;
using System.Collections.Generic;
using NUnit.Framework;
using ReMi.DataEntities.BusinessRules;
using ReMi.DataAccess.Helpers;
using System.Linq;

namespace ReMi.DataAccess.Tests.Helpers
{
    [TestFixture]
    public class BusinessRuleCollectorTests
    {
        [Test]
        public void Collect_ShouldNotBeEmptyResult_WhenInvoked()
        {
            var result = (IList<BusinessRuleDescription>)BusinessRuleCollector.Collect();

            Assert.IsNotEmpty(result);
            Assert.IsTrue(result.All(x => !string.IsNullOrEmpty(x.Description)), "Some rule 'Description' is null or empty");
            Assert.IsTrue(result.All(x => !string.IsNullOrEmpty(x.Name)), "Some rule 'Name' is null or empty");
            Assert.IsTrue(result.All(x => x.ExternalId != Guid.Empty), "Some rule 'ExternalId' is empty");
            Assert.IsTrue(
                result.Where(x => x.Parameters != null)
                    .SelectMany(x => x.Parameters)
                    .All(x => !string.IsNullOrEmpty(x.Type)), "Some parameter 'Type' is null or empty");
            Assert.IsTrue(
                result.Where(x => x.Parameters != null)
                    .SelectMany(x => x.Parameters)
                    .All(x => !string.IsNullOrEmpty(x.Name)), "Some parameter 'Name' is null or empty");
            Assert.IsTrue(
                result.Where(x => x.Parameters != null)
                    .SelectMany(x => x.Parameters)
                    .All(x => x.ExternalId != Guid.Empty), "Some parameter 'ExternalId' is empty");
            Assert.IsTrue(result.All(x => x.Parameters == null || x.Parameters.All(p => p.BusinessRule == x)), "Some parameter has empty or incorrect business rule assign");
        }
    }
}
