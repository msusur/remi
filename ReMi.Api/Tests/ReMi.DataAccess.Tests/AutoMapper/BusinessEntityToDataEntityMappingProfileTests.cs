using AutoMapper;
using FizzWare.NBuilder;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.BusinessRules;
using ReMi.BusinessEntities.ProductRequests;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Common.Constants.BusinessRules;
using ReMi.Common.Constants.ReleasePlan;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.AutoMapper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ReMi.BusinessEntities.DeploymentTool;
using ReMi.Contracts.Plugins.Data.SourceControl;

namespace ReMi.DataAccess.Tests.AutoMapper
{
    [TestFixture]
    public class BusinessEntityToDataEntityMappingProfileTests : TestClassFor<IMappingEngine>
    {
        protected override IMappingEngine ConstructSystemUnderTest()
        {
            Mapper.Initialize(
                c => c.AddProfile<BusinessEntityToDataEntityMappingProfile>());

            return Mapper.Engine;
        }

        [Test]
        public void ReleaseContentMapping_ShouldReturnTicket_WhenMapInvoked()
        {
            var ticket = Builder<ReleaseContentTicket>.CreateNew()
                .With(x => x.Comment, RandomData.RandomString(10))
                .With(x => x.TicketDescription, RandomData.RandomString(10))
                .With(x => x.Assignee, RandomData.RandomString(10))
                .With(x => x.TicketId, Guid.NewGuid())
                .With(x => x.TicketName, "RM-" + RandomData.RandomInt(4))
                .With(x => x.Risk, RandomData.RandomEnum<TicketRisk>())
                .With(x => x.LastChangedByAccount, Guid.NewGuid())
                .Build();

            var expected = Builder<DataEntities.ReleasePlan.ReleaseContent>.CreateNew()
                .With(x => x.Comment, ticket.Comment)
                .With(x => x.Description, ticket.TicketDescription)
                .With(x => x.Assignee, ticket.Assignee)
                .With(x => x.TicketKey, ticket.TicketName)
                .With(x => x.TicketId, ticket.TicketId)
                .With(x => x.TicketRisk, ticket.Risk)
                .With(x => x.LastChangedByAccount, Builder<DataEntities.Auth.Account>.CreateNew()
                    .With(y => y.ExternalId, ticket.LastChangedByAccount)
                    .Build())
                .Build();

            var actual = Sut.Map<ReleaseContentTicket, DataEntities.ReleasePlan.ReleaseContent>(ticket);

            Assert.AreEqual(expected.Comment, actual.Comment);
            Assert.AreEqual(expected.Description, actual.Description);
            Assert.AreEqual(expected.Assignee, actual.Assignee);
            Assert.AreEqual(expected.TicketId, actual.TicketId);
            Assert.AreEqual(expected.TicketKey, actual.TicketKey);
            Assert.AreEqual(expected.TicketRisk, actual.TicketRisk);
            Assert.AreEqual(expected.LastChangedByAccount.ExternalId, actual.LastChangedByAccount.ExternalId);
        }

        [Test]
        public void RoleMapping_ShouldReturnRole_WhenMapInvoked()
        {
            var role = Builder<Role>.CreateNew()
                .With(x => x.Name, RandomData.RandomString(10))
                .With(x => x.Description, RandomData.RandomString(10))
                .With(x => x.ExternalId, Guid.NewGuid())
                .Build();

            var expected = Builder<DataEntities.Auth.Role>.CreateNew()
                .With(x => x.Name, role.Name)
                .With(x => x.Description, role.Description)
                .With(x => x.ExternalId, role.ExternalId)
                .Build();

            var actual = Sut.Map<Role, DataEntities.Auth.Role>(role);

            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.Description, actual.Description);
            Assert.AreEqual(expected.ExternalId, actual.ExternalId);
            Assert.AreEqual(0, actual.Id);
        }

        [Test]
        public void ProductRequestType_ShouldReturnValidType_WhenMapInvoked()
        {
            var source = Builder<ProductRequestType>.CreateNew()
                .With(x => x.Name, RandomData.RandomString(100))
                .With(x => x.ExternalId, Guid.NewGuid())
                .With(x => x.RequestGroups,
                    Builder<ProductRequestGroup>.CreateListOfSize(1)
                        .All()
                        .With(o => o.ExternalId)
                        .Build()
                )
                .Build();

            var actual = Sut.Map<ProductRequestType, DataEntities.ProductRequests.ProductRequestType>(source);

            Assert.IsNull(actual.RequestGroups, "request groups");
            Assert.AreEqual(source.Name, actual.Name, "name");
            Assert.AreEqual(source.ExternalId, actual.ExternalId, "external id");
        }

        [Test]
        public void ProductRequestGroupAssignee_ShouldReturnValidType_WhenMapInvoked()
        {
            var source = Builder<Account>.CreateNew()
                .With(x => x.FullName, RandomData.RandomString(100))
                .With(x => x.ExternalId, Guid.NewGuid())
                .With(x => x.Email, RandomData.RandomEmail())
                .Build();

            var actual = Sut.Map<Account, DataEntities.ProductRequests.ProductRequestGroupAssignee>(source);

            Assert.IsNull(actual.RequestGroup, "request group");
            Assert.IsNotNull(actual.Account, "account");
            Assert.AreEqual(source.ExternalId, actual.Account.ExternalId, "external id");
            Assert.AreEqual(source.FullName, actual.Account.FullName, "full name");
        }
        [Test]
        public void BusinessRuleParameterMapping_ShouldReturnProperType_WhenConvertingToBusinessType()
        {
            var parameter = new BusinessRuleParameter
            {
                ExternalId = Guid.NewGuid(),
                Name = RandomData.RandomString(10),
                Type = "int",
                TestData = new BusinessRuleTestData
                {
                    ExternalId = Guid.NewGuid(),
                    JsonData = RandomData.RandomString(10)
                }
            };

            var actual = Sut.Map<BusinessRuleParameter, DataEntities.BusinessRules.BusinessRuleParameter>(parameter);

            Assert.AreEqual(parameter.Type, actual.Type);
            Assert.AreEqual(parameter.ExternalId, actual.ExternalId);
            Assert.AreEqual(parameter.Name, actual.Name);
            Assert.AreEqual(parameter.TestData.JsonData, actual.TestData.JsonData);
            Assert.AreEqual(parameter.TestData.ExternalId, actual.TestData.ExternalId);
        }

        [Test]
        public void BusinessRuleTestDataMapping_ShouldReturnProperType_WhenConvertingToBusinessType()
        {
            var testData = new BusinessRuleTestData
            {
                ExternalId = Guid.NewGuid(),
                JsonData = RandomData.RandomString(10)
            };

            var actual = Sut.Map<BusinessRuleTestData, DataEntities.BusinessRules.BusinessRuleTestData>(testData);

            Assert.AreEqual(testData.JsonData, actual.JsonData);
            Assert.AreEqual(testData.ExternalId, actual.ExternalId);
        }

        [Test]
        public void BusinessRuleAccountTestDataMapping_ShouldReturnProperType_WhenConvertingToBusinessType()
        {
            var testData = new BusinessRuleAccountTestData
            {
                ExternalId = Guid.NewGuid(),
                JsonData = RandomData.RandomString(10)
            };

            var actual = Sut.Map<BusinessRuleAccountTestData, DataEntities.BusinessRules.BusinessRuleAccountTestData>(testData);

            Assert.AreEqual(testData.JsonData, actual.JsonData);
            Assert.AreEqual(testData.ExternalId, actual.ExternalId);
        }

        [Test]
        public void BusinessRuleDescriptionMapping_ShouldReturnProperEnumType_WhenConvertingToBusinessType()
        {
            var rule = new BusinessRuleDescription
            {
                Script = RandomData.RandomString(10),
                Group = RandomData.RandomEnum<BusinessRuleGroup>(),
                ExternalId = Guid.NewGuid(),
                Parameters = new List<BusinessRuleParameter>
                {
                    new BusinessRuleParameter
                    {
                        ExternalId = Guid.NewGuid(),
                        Name = RandomData.RandomString(10),
                        Type = "System.Int32"
                    }
                }
            };

            var actual = Sut.Map<BusinessRuleDescription, DataEntities.BusinessRules.BusinessRuleDescription>(rule);

            Assert.AreEqual(rule.Group, actual.Group);
            Assert.AreEqual(rule.ExternalId, actual.ExternalId);
            Assert.AreEqual(rule.Script, actual.Script);
            Assert.AreEqual(rule.Parameters.Count(), actual.Parameters.Count);
        }

        [Test]
        public void ProductRequestRegistration()
        {
            var source = Builder<ProductRequestRegistration>.CreateNew()
                .With(x => x.Description, RandomData.RandomString(100))
                .With(x => x.ExternalId, Guid.NewGuid())
                .With(x => x.CreatedOn, DateTime.Now)
                .With(x => x.CreatedBy, RandomData.RandomString(1, 20))
                .With(x => x.CreatedByAccountId, Guid.NewGuid())
                .With(x => x.Tasks,
                    Builder<ProductRequestRegistrationTask>.CreateListOfSize(RandomData.RandomInt(1, 5))
                        .All()
                        .With(o => o.ProductRequestTaskId, Guid.NewGuid())
                        .Build())
                .Build();

            var actual = Sut.Map<ProductRequestRegistration, DataEntities.ProductRequests.ProductRequestRegistration>(source);

            Assert.IsNotNull(actual, "request registration");
            Assert.AreEqual(source.Description, actual.Description);
            Assert.AreEqual(source.ExternalId, actual.ExternalId);
            Assert.IsNull(actual.CreatedBy);
            Assert.AreEqual(source.CreatedOn, actual.CreatedOn.ToLocalTime());
            Assert.AreEqual(DateTimeKind.Utc, actual.CreatedOn.Kind);

            Assert.IsNull(actual.Tasks, "tasks");
        }

        [Test]
        public void ProductRequestRegistrationTask()
        {
            var source = Builder<ProductRequestRegistrationTask>.CreateNew()
                .With(x => x.IsCompleted, RandomData.RandomBool())
                .With(x => x.ProductRequestTaskId, Guid.NewGuid())
                .With(x => x.LastChangedOn, DateTime.Now)
                .With(x => x.LastChangedBy, RandomData.RandomString(1, 20))
                .With(x => x.LastChangedByAccountId, Guid.NewGuid())
                .Build();

            var actual = Sut.Map<ProductRequestRegistrationTask, DataEntities.ProductRequests.ProductRequestRegistrationTask>(source);

            Assert.IsNotNull(actual, "request registration task");
            Assert.AreEqual(source.IsCompleted, actual.IsCompleted);
            Assert.IsNull(actual.ProductRequestTask);
            Assert.IsNull(actual.LastChangedBy);
            Assert.IsNull(actual.LastChangedOn);
        }
        
        [Test]
        public void ReleaseJob_ShouldReturnDataReleaseJob_WhenMapFromBusinessEntity()
        {
            var releaseJob = new ReleaseJob
            {
                ExternalId = Guid.NewGuid(),
                IsIncluded = RandomData.RandomBool(),
                JobId = Guid.NewGuid(),
                Name = RandomData.RandomString(10),
                Order = RandomData.RandomInt(100)
            };

            var result = Sut.Map<ReleaseJob, DataEntities.ReleasePlan.ReleaseJob>(releaseJob);

            Assert.AreEqual(releaseJob.ExternalId, result.ExternalId);
            Assert.AreEqual(releaseJob.IsIncluded, result.IsIncluded);
            Assert.AreEqual(releaseJob.JobId, result.JobId);
            Assert.AreEqual(releaseJob.Name, result.Name);
            Assert.AreEqual(releaseJob.Order, result.Order);
        }
    }
}
