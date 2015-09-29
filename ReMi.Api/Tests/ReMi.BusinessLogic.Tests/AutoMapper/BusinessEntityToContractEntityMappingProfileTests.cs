using AutoMapper;
using NUnit.Framework;
using ReMi.BusinessLogic.AutoMapper;
using System;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.TestUtils.UnitTests;

namespace ReMi.BusinessLogic.Tests.AutoMapper
{
    [TestFixture]
    public class BusinessEntityToContractEntityMappingProfileTests : TestClassFor<IMappingEngine>
    {
        protected override IMappingEngine ConstructSystemUnderTest()
        {
            Mapper.Initialize(
                c => c.AddProfile<BusinessEntityToContractEntityMappingProfile>());

            return Mapper.Engine;
        }

        [Test]
        public void ReleaseContentTicket_ShouldReturnContractReleaseContentTicket_WhenMapFromBusinessEntity()
        {
            var releaseContentTicket = new ReleaseContentTicket
            {
                Assignee = RandomData.RandomString(10),
                TicketDescription = RandomData.RandomString(10),
                TicketId = Guid.NewGuid(),
                TicketName = RandomData.RandomString(10),
                TicketUrl = RandomData.RandomString(10)
            };

            var result = Sut.Map<ReleaseContentTicket, Contracts.Plugins.Data.ReleaseContent.ReleaseContentTicket>(releaseContentTicket);

            Assert.AreEqual(releaseContentTicket.Assignee, result.Assignee);
            Assert.AreEqual(releaseContentTicket.TicketDescription, result.TicketDescription);
            Assert.AreEqual(releaseContentTicket.TicketId, result.TicketId);
            Assert.AreEqual(releaseContentTicket.TicketName, result.TicketName);
            Assert.AreEqual(releaseContentTicket.TicketUrl, result.TicketUrl);
        }

        [Test]
        public void ReleaseTask_ShouldReturnContractHelpDeskTicket_WhenMapFromBusinessEntity()
        {
            var releaseTask = new ReleaseTask
            {
                Assignee = RandomData.RandomString(10),
                ReleaseWindowId = Guid.NewGuid(),
                ExternalId = Guid.NewGuid(),
                HelpDeskTicketReference = RandomData.RandomString(5),
                HelpDeskTicketUrl = RandomData.RandomString(10),
                Description = RandomData.RandomString(10)
            };

            var result = Sut.Map<ReleaseTask, Contracts.Plugins.Data.HelpDesk.HelpDeskTicket>(releaseTask);

            Assert.AreEqual(releaseTask.Description, result.Description);
            Assert.AreEqual(releaseTask.HelpDeskTicketReference, result.Id);
            Assert.IsNull(result.Priority);
            Assert.IsNull(result.Subject);
            Assert.IsNull(result.Url);
            Assert.AreEqual(releaseTask.Description, result.Comment);
        }

    }
}
