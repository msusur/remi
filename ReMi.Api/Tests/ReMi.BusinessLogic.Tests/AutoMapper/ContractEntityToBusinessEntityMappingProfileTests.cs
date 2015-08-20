using AutoMapper;
using FizzWare.NBuilder;
using NUnit.Framework;
using ReMi.BusinessEntities.DeploymentTool;
using ReMi.BusinessEntities.HelpDesk;
using ReMi.BusinessLogic.AutoMapper;
using ReMi.Common.Utils.UnitTests;
using ReMi.Contracts.Plugins.Data.HelpDesk;
using System;
using System.Linq;

namespace ReMi.BusinessLogic.Tests.AutoMapper
{
    [TestFixture]
    public class ContractEntityToBusinessEntityMappingProfileTests : TestClassFor<IMappingEngine>
    {
        protected override IMappingEngine ConstructSystemUnderTest()
        {
            Mapper.Initialize(
                c => c.AddProfile<ContractEntityToBusinessEntityMappingProfile>());

            return Mapper.Engine;
        }

        [Test]
        public void ReleaseContentTicket_ShouldReturnBusinessReleaseContent_WhenMapFromContractReleaseContent()
        {
            var releaseContentTicket = new Contracts.Plugins.Data.ReleaseContent.ReleaseContentTicket
            {
                Assignee = RandomData.RandomString(10),
                TicketDescription = RandomData.RandomString(10),
                TicketId = Guid.NewGuid(),
                TicketName = RandomData.RandomString(10),
                TicketUrl = RandomData.RandomString(10)
            };

            var result = Sut.Map<Contracts.Plugins.Data.ReleaseContent.ReleaseContentTicket, BusinessEntities.ReleasePlan.ReleaseContentTicket>(releaseContentTicket);

            Assert.AreEqual(releaseContentTicket.Assignee, result.Assignee);
            Assert.AreEqual(releaseContentTicket.TicketDescription, result.TicketDescription);
            Assert.AreEqual(releaseContentTicket.TicketId, result.TicketId);
            Assert.AreEqual(releaseContentTicket.TicketName, result.TicketName);
            Assert.AreEqual(releaseContentTicket.TicketUrl, result.TicketUrl);
        }

        [Test]
        public void ReleaseJob_ShouldReturnReleaseJob_WhenMapFromContractReleaseJob()
        {
            var releaseJob = new Contracts.Plugins.Data.DeploymentTool.ReleaseJob
            {
                ExternalId = Guid.NewGuid(),
                IsDisabled = RandomData.RandomBool(),
                IsIncluded = RandomData.RandomBool(),
                Name = RandomData.RandomString(10),
                Order = RandomData.RandomInt(int.MaxValue)
            };

            var result = Sut.Map<Contracts.Plugins.Data.DeploymentTool.ReleaseJob, ReleaseJob>(releaseJob);

            Assert.AreEqual(releaseJob.ExternalId, result.JobId);
            Assert.AreEqual(releaseJob.IsIncluded, result.IsIncluded);
            Assert.AreEqual(releaseJob.Name, result.Name);
            Assert.AreEqual(releaseJob.Order, result.Order);
        }

        [Test]
        public void JobMetric_ShouldReturnJobMeasurement_WhenMapFromContractReleaseJob()
        {
            var jobMetric = new Contracts.Plugins.Data.DeploymentTool.JobMetric
            {
                Name = RandomData.RandomString(10),
                BuildNumber = RandomData.RandomInt(int.MaxValue),
                JobId = RandomData.RandomString(10),
                ChildMetrics = Builder<Contracts.Plugins.Data.DeploymentTool.JobMetric>.CreateListOfSize(5).Build(),
                EndTime = RandomData.RandomDateTime(20000000, 30000000),
                StartTime = RandomData.RandomDateTime(10000000, 20000000),
                Url = RandomData.RandomString(10),
                NumberOfTries = RandomData.RandomInt(int.MaxValue)
            };

            var result = Sut.Map<Contracts.Plugins.Data.DeploymentTool.JobMetric, JobMeasurement>(jobMetric);

            Assert.AreEqual(jobMetric.Name, result.StepName);
            Assert.AreEqual(jobMetric.JobId, result.StepId);
            Assert.AreEqual(jobMetric.BuildNumber, result.BuildNumber);
            Assert.AreEqual(jobMetric.NumberOfTries, result.NumberOfTries);
            Assert.AreEqual(jobMetric.ChildMetrics.Count(), result.ChildSteps.Count());
            Assert.AreEqual(jobMetric.EndTime, result.FinishTime);
            Assert.AreEqual(jobMetric.StartTime, result.StartTime);
            Assert.AreEqual(jobMetric.Url, result.Locator);
        }

        [Test]
        public void HelpDeskTicket_ShouldReturnBusinessHelpDeskTaskView_WhenMapFromContractEntity()
        {
            var task = new HelpDeskTicket
            {
                Subject = RandomData.RandomString(10),
                Url = RandomData.RandomString(10),
                Comment = RandomData.RandomString(10),
                Id = RandomData.RandomString(5),
                Priority = "low",
                Description = RandomData.RandomString(10)
            };

            var result = Sut.Map<HelpDeskTicket, HelpDeskTaskView>(task);

            Assert.AreEqual(task.Subject, result.Description);
            Assert.AreEqual(task.Id, result.Number);
            Assert.AreEqual(task.Url, result.LinkUrl);
        }

        [Test]
        public void HelpDeskTicket_ShouldReturnBusinessHelpDeskTask_WhenMapFromContractEntity()
        {
            var task = new HelpDeskTicket
            {
                Subject = RandomData.RandomString(10),
                Url = RandomData.RandomString(10),
                Comment = RandomData.RandomString(10),
                Id = RandomData.RandomString(5),
                Priority = "low",
                Description = RandomData.RandomString(10)
            };

            var result = Sut.Map<HelpDeskTicket, HelpDeskTask>(task);

            Assert.AreEqual(task.Description, result.Description);
            Assert.AreEqual(task.Subject, result.Subject);
            Assert.AreEqual(task.Id, result.Number);
            Assert.AreEqual(task.Url, result.Url);
        }
    }
}
