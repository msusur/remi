using AutoMapper;
using FizzWare.NBuilder;
using NUnit.Framework;
using ReMi.BusinessEntities.DeploymentTool;
using ReMi.Common.Constants.BusinessRules;
using ReMi.Common.Constants.ProductRequests;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Constants.ReleaseExecution;
using ReMi.Common.Constants.ReleasePlan;
using ReMi.Common.Utils;
using ReMi.Common.Utils.Enums;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Data;
using ReMi.DataAccess.AutoMapper;
using ReMi.DataEntities.Api;
using ReMi.DataEntities.BusinessRules;
using ReMi.DataEntities.Metrics;
using ReMi.DataEntities.Plugins;
using ReMi.DataEntities.ProductRequests;
using ReMi.DataEntities.Products;
using ReMi.DataEntities.ReleaseCalendar;
using ReMi.DataEntities.ReleasePlan;
using System;
using System.Collections.Generic;
using System.Linq;
using BusinessAccount = ReMi.BusinessEntities.Auth.Account;
using BusinessBusinessUnit = ReMi.BusinessEntities.Products.BusinessUnit;
using BusinessProduct = ReMi.BusinessEntities.Products.Product;
using BusinessProductView = ReMi.BusinessEntities.Products.ProductView;
using BusinessReleaseWindow = ReMi.BusinessEntities.ReleaseCalendar.ReleaseWindow;
using CommandPermission = ReMi.DataEntities.Auth.CommandPermission;
using DataAccount = ReMi.DataEntities.Auth.Account;
using DataAccountProduct = ReMi.DataEntities.Auth.AccountProduct;
using DataProduct = ReMi.DataEntities.Products.Product;
using DataReleaseNote = ReMi.DataEntities.ReleaseCalendar.ReleaseNote;
using DataReleaseType = ReMi.Common.Constants.ReleaseCalendar.ReleaseType;
using DataReleaseWindow = ReMi.DataEntities.ReleaseCalendar.ReleaseWindow;
using ReleaseJob = ReMi.DataEntities.ReleasePlan.ReleaseJob;
using Role = ReMi.DataEntities.Auth.Role;
using SignOff = ReMi.DataEntities.ReleaseExecution.SignOff;

namespace ReMi.DataAccess.Tests.AutoMapper
{
    [TestFixture]
    public class DataEntityToBusinessEntityMappingProfileTest : TestClassFor<IMappingEngine>
    {
        protected override IMappingEngine ConstructSystemUnderTest()
        {
            Mapper.Initialize(
                c => c.AddProfile(new DataEntityToBusinessEntityMappingProfile()));

            return Mapper.Engine;
        }

        [Test]
        public void ProductMapping_ShouldReturnMappedProduct_WhenMapInvoked()
        {
            var product = CreateProduct();

            var expected = CreateProduct(product);

            var actual = Sut.Map<DataProduct, BusinessProduct>(product);

            Assert.AreEqual(expected.ExternalId, actual.ExternalId);
            Assert.AreEqual(expected.Description, actual.Description);
            Assert.AreEqual(expected.BusinessUnit.ExternalId, actual.BusinessUnit.ExternalId);
        }

        [Test]
        public void ProductToProductViewMapping_ShouldReturnMappedProductView_WhenMapInvoked()
        {
            var product = CreateProduct();

            var actual = Sut.Map<DataProduct, BusinessProductView>(product);

            Assert.AreEqual(product.ExternalId, actual.ExternalId);
            Assert.AreEqual(product.Description, actual.Name);
        }

        [Test]
        public void BusinessUnitMapping_ShouldReturnMappedBusinessUnit_WhenMapInvoked()
        {
            var businessUnit = new BusinessUnit
            {
                ExternalId = Guid.NewGuid(),
                BusinessUnitId = RandomData.RandomInt(int.MaxValue),
                Description = RandomData.RandomString(30),
                Name = RandomData.RandomString(10),
                Packages = Builder<Product>.CreateListOfSize(5)
                    .All()
                    .Do(x => x.ExternalId = Guid.NewGuid())
                    .Build()
            };

            var actual = Sut.Map<BusinessUnit, BusinessBusinessUnit>(businessUnit);

            Assert.AreEqual(businessUnit.ExternalId, actual.ExternalId);
            Assert.AreEqual(businessUnit.Description, actual.Description);
            Assert.AreEqual(businessUnit.Name, actual.Name);
            Assert.IsNull(actual.Packages);
        }

        [Test]
        public void AccountMapping_ShouldReturnMappedAccountWithProducts_WhenMapInvoked()
        {
            var account = CreateAccount();

            var actual = Sut.Map<DataAccount, BusinessAccount>(account);

            Assert.AreEqual(account.ExternalId, actual.ExternalId);
            Assert.AreEqual(account.CreatedOn, actual.CreatedOn);
            Assert.AreEqual(account.Description, actual.Description);
            Assert.AreEqual(account.Email, actual.Email);
            Assert.AreEqual(account.FullName, actual.FullName);
            Assert.AreEqual(account.IsBlocked, actual.IsBlocked);
            Assert.AreEqual(account.Name, actual.Name);
            Assert.AreEqual(account.Role.Name, actual.Role.Name);
            CollectionAssertHelper.AreEqual(account.AccountProducts, actual.Products, (e, a) =>
                e.Product.ExternalId == a.ExternalId
                && e.Product.Description == a.Name
                && e.IsDefault == a.IsDefault);
        }

        [Test]
        public void AccountMapping_ShouldReturnMappedAccountWithoutProducts_WhenMapInvokedWithoutProducts()
        {
            var account = CreateAccount();
            account.AccountProducts = null;

            var actual = Sut.Map<DataAccount, BusinessAccount>(account);

            Assert.IsEmpty(actual.Products);
        }

        [Test]
        public void ReleaseWindowMapping_ShouldReturnReleaseWindow_WhenMapInvoked()
        {
            var releaseWindow = CreateReleaseWindow();

            var expected = CreateReleaseWindow(releaseWindow);

            var actual = Sut.Map<DataReleaseWindow, BusinessReleaseWindow>(releaseWindow);

            Assert.AreEqual(expected.ExternalId, actual.ExternalId);
            Assert.AreEqual(expected.ReleaseType, actual.ReleaseType);
            Assert.AreEqual(expected.StartTime, actual.StartTime);
            Assert.AreEqual(expected.OriginalStartTime, actual.OriginalStartTime);
            Assert.AreEqual(expected.Sprint, actual.Sprint);
            Assert.AreEqual(expected.RequiresDowntime, actual.RequiresDowntime);
            Assert.AreEqual(expected.ReleaseDecision, actual.ReleaseDecision);
            Assert.AreEqual(expected.IsFailed, actual.IsFailed);
            Assert.AreEqual(EnumDescriptionHelper.GetDescription(releaseWindow.ReleaseType), actual.ReleaseTypeDescription);
            Assert.AreEqual(EnumDescriptionHelper.GetDescription(releaseWindow.ReleaseDecision), actual.ReleaseDecision);
            if (expected.ClosedOn != null) Assert.AreEqual(expected.ClosedOn.Value.ToLocalTime(), actual.ClosedOn);
            if (expected.ApprovedOn != null) Assert.AreEqual(expected.ApprovedOn.Value.ToLocalTime(), actual.ApprovedOn);
            Assert.IsNull(actual.ReleaseNotes);
            CollectionAssert.AreEquivalent(releaseWindow.ReleaseProducts.Select(x => x.Product.Description).ToList(), actual.Products);
        }

        [Test]
        public void ReleaseContentMapping_ShouldReturnTicket_WhenMapInvoked()
        {
            var releaseContent = Builder<ReleaseContent>.CreateNew()
                .With(x => x.Comment, RandomData.RandomString(10))
                .With(x => x.Description, RandomData.RandomString(10))
                .With(x => x.Assignee, RandomData.RandomString(10))
                .With(x => x.TicketId, Guid.NewGuid())
                .With(x => x.TicketKey, "RM-" + RandomData.RandomInt(4))
                .With(x => x.TicketRisk, RandomData.RandomEnum<TicketRisk>())
                .With(x => x.TicketUrl, RandomData.RandomString(10))
                .With(x => x.LastChangedByAccount, Builder<DataAccount>.CreateNew()
                    .With(y => y.ExternalId, Guid.NewGuid())
                    .Build())
                .Build();

            var expected = Builder<BusinessEntities.ReleasePlan.ReleaseContentTicket>.CreateNew()
                .With(x => x.Comment, releaseContent.Comment)
                .With(x => x.TicketDescription, releaseContent.Description)
                .With(x => x.Assignee, releaseContent.Assignee)
                .With(x => x.TicketName, releaseContent.TicketKey)
                .With(x => x.TicketId, releaseContent.TicketId)
                .With(x => x.Risk, releaseContent.TicketRisk)
                .With(x => x.TicketUrl, releaseContent.TicketUrl)
                .With(x => x.LastChangedByAccount, releaseContent.LastChangedByAccount.ExternalId)
                .Build();

            var actual = Sut.Map<ReleaseContent, BusinessEntities.ReleasePlan.ReleaseContentTicket>(releaseContent);

            Assert.AreEqual(expected.Comment, actual.Comment);
            Assert.AreEqual(expected.Assignee, actual.Assignee);
            Assert.AreEqual(expected.TicketDescription, actual.TicketDescription);
            Assert.AreEqual(expected.TicketId, actual.TicketId);
            Assert.AreEqual(expected.TicketName, actual.TicketName);
            Assert.AreEqual(expected.Risk, actual.Risk);
            Assert.AreEqual(expected.LastChangedByAccount, actual.LastChangedByAccount);
            Assert.AreEqual(expected.TicketUrl, actual.TicketUrl);
        }

        [Test]
        public void CommandMapping_ShouldReturnBusinessCommand_WhenMapInvoked()
        {
            var command = Builder<Command>.CreateNew()
                .With(x => x.Name, RandomData.RandomString(10))
                .With(x => x.Description, RandomData.RandomString(10))
                .With(x => x.IsBackground, RandomData.RandomBool())
                .With(x => x.CommandId, RandomData.RandomInt(6))
                .With(x => x.Group, RandomData.RandomString(10))
                .With(x => x.CommandPermissions, Builder<CommandPermission>.CreateListOfSize(3)
                    .All()
                    .Do(y => y.Role = Builder<Role>.CreateNew()
                        .With(z => z.ExternalId, Guid.NewGuid())
                        .With(z => z.Description, RandomData.RandomString(10))
                        .With(z => z.Id, RandomData.RandomInt(10000))
                        .With(z => z.Name, RandomData.RandomString(10))
                        .Build())
                    .Build())
                .Build();

            var expected = Builder<BusinessEntities.Api.Command>.CreateNew()
                .With(x => x.Name, command.Name)
                .With(x => x.Description, command.Description)
                .With(x => x.IsBackground, command.IsBackground)
                .With(x => x.CommandId, command.CommandId)
                .With(x => x.Group, command.Group)
                .With(x => x.Roles, command.CommandPermissions.Select(y => new BusinessEntities.Auth.Role
                {
                    Description = y.Role.Description,
                    Name = y.Role.Name,
                    ExternalId = y.Role.ExternalId
                }))
                .Build();

            var actual = Sut.Map<Command, BusinessEntities.Api.Command>(command);

            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.Description, actual.Description);
            Assert.AreEqual(expected.IsBackground, actual.IsBackground);
            Assert.AreEqual(expected.CommandId, actual.CommandId);
            Assert.AreEqual(expected.Group, actual.Group);
            Assert.AreEqual(expected.Roles.Count(), actual.Roles.Count());

            CollectionAssertHelper.AreEqual<BusinessEntities.Auth.Role, BusinessEntities.Auth.Role>(expected.Roles, actual.Roles, (e, a) =>
                e.Description == a.Description
                && e.Name == a.Name
                && e.ExternalId == a.ExternalId);
        }

        [Test]
        public void RoleMapping_ShouldReturnRole_WhenMapInvoked()
        {
            var role = Builder<Role>.CreateNew()
                .With(x => x.Name, RandomData.RandomString(10))
                .With(x => x.Description, RandomData.RandomString(10))
                .With(x => x.ExternalId, Guid.NewGuid())
                .With(x => x.Id, RandomData.RandomInt(10000))
                .Build();

            var expected = Builder<BusinessEntities.Auth.Role>.CreateNew()
                .With(x => x.Name, role.Name)
                .With(x => x.Description, role.Description)
                .With(x => x.ExternalId, role.ExternalId)
                .Build();

            var actual = Sut.Map<Role, BusinessEntities.Auth.Role>(role);

            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.Description, actual.Description);
            Assert.AreEqual(expected.ExternalId, actual.ExternalId);
        }

        [Test]
        public void SignOffMapping_ShouldReturnSigner_WhenMapInvoked()
        {
            var signer = Builder<SignOff>.CreateNew()
                .With(x => x.SignedOff, SystemTime.Now)
                .With(x => x.Account, new DataAccount { ExternalId = Guid.NewGuid() })
                .With(x => x.ExternalId, Guid.NewGuid())
                .Build();

            var expected = Builder<BusinessEntities.ReleaseExecution.SignOff>.CreateNew()
                .With(x => x.SignedOff, true)
                .With(x => x.Signer, new BusinessAccount { ExternalId = signer.Account.ExternalId })
                .With(x => x.ExternalId, signer.ExternalId)
                .Build();

            var actual = Sut.Map<SignOff, BusinessEntities.ReleaseExecution.SignOff>(signer);

            Assert.AreEqual(expected.Signer.ExternalId, actual.Signer.ExternalId);
            Assert.AreEqual(expected.SignedOff, actual.SignedOff);
            Assert.AreEqual(expected.ExternalId, actual.ExternalId);
        }

        [Test]
        public void MetricMapping_ShouldReturnMetric_WhenMapInvoked()
        {
            var metric = Builder<Metric>.CreateNew()
                .With(x => x.ExecutedOn, DateTime.UtcNow)
                .With(x => x.MetricType, MetricType.SiteDown)
                .With(x => x.ExternalId, Guid.NewGuid())
                .Build();

            var expected = Builder<BusinessEntities.Metrics.Metric>.CreateNew()
                .With(x => x.ExecutedOn, metric.ExecutedOn)
                .With(x => x.MetricType, MetricType.SiteDown)
                .With(x => x.Order, 3)
                .With(x => x.ExternalId, metric.ExternalId)
                .Build();

            var actual = Sut.Map<Metric, BusinessEntities.Metrics.Metric>(metric);

            Assert.AreEqual(expected.ExecutedOn.Value.ToLocalTime(), actual.ExecutedOn, "executed on");
            Assert.AreEqual(expected.ExternalId, actual.ExternalId, "external id");
            Assert.AreEqual(expected.MetricType, actual.MetricType, "metric type");
            Assert.AreEqual(expected.Order, actual.Order, "order");
        }

        [Test]
        public void ReleaseDeploymentMeasurement_ShouldReturnJobMeasurement_WhenMapInvoked()
        {
            var dt = RandomData.RandomDateTime(DateTimeKind.Utc);

            var source = Builder<ReleaseDeploymentMeasurement>.CreateNew()
                .With(o => o.StartTime, dt.AddMinutes(-RandomData.RandomInt(1, 100)))
                .With(o => o.StartTime, dt.AddMinutes(RandomData.RandomInt(1, 100)))
                .With(o => o.StepId, RandomData.RandomString(10))
                .With(o => o.StepName, RandomData.RandomString(20))
                .With(o => o.Locator, RandomData.RandomString(50))
                .Build();

            var expected = Builder<ReleaseDeploymentMeasurement>.CreateNew()
                .With(o => o.StartTime, source.StartTime.Value.ToLocalTime())
                .With(o => o.FinishTime, source.FinishTime.Value.ToLocalTime())
                .With(o => o.StepId, source.StepId)
                .With(o => o.StepName, source.StepName)
                .With(o => o.Locator, source.Locator)
                .Build();

            var actual = Sut.Map<ReleaseDeploymentMeasurement, JobMeasurement>(source);
            Assert.AreEqual(expected.FinishTime.Value.Subtract(expected.StartTime.Value).TotalMilliseconds, actual.Duration, "duration");
            Assert.AreEqual(expected.Locator, actual.Locator, "locator");
            Assert.AreEqual(expected.StartTime, actual.StartTime, "start time");
            Assert.AreEqual(DateTimeKind.Local, actual.StartTime.Value.Kind, "local start time");
            Assert.AreEqual(expected.FinishTime, actual.FinishTime, "finish time");
            Assert.AreEqual(DateTimeKind.Local, actual.FinishTime.Value.Kind, "local finish time");
            Assert.AreEqual(expected.StepName, actual.StepName, "step name");
            Assert.AreEqual(expected.StepId, actual.StepId, "step id");
            Assert.AreEqual(expected.BuildNumber, actual.BuildNumber, "BuildNumber");
            Assert.AreEqual(expected.NumberOfTries, actual.NumberOfTries, "NumberOfTries");
            Assert.IsNull(actual.ChildSteps, "child steps");
        }

        [Test]
        public void BusinessRuleParameterMapping_ShouldReturnProperType_WhenConvertingToBusinessType()
        {
            var parameter = new BusinessRuleParameter
            {
                BusinessRuleId = 1,
                ExternalId = Guid.NewGuid(),
                Name = RandomData.RandomString(10),
                Type = "int",
                TestData = new BusinessRuleTestData
                {
                    BusinessRuleTestDataId = 1,
                    ExternalId = Guid.NewGuid(),
                    JsonData = RandomData.RandomString(10)
                }
            };

            var actual = Sut.Map<BusinessRuleParameter, BusinessEntities.BusinessRules.BusinessRuleParameter>(parameter);

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
                BusinessRuleTestDataId = 1,
                ExternalId = Guid.NewGuid(),
                JsonData = RandomData.RandomString(10)
            };

            var actual = Sut.Map<BusinessRuleTestData, BusinessEntities.BusinessRules.BusinessRuleTestData>(testData);

            Assert.AreEqual(testData.JsonData, actual.JsonData);
            Assert.AreEqual(testData.ExternalId, actual.ExternalId);
        }

        [Test]
        public void BusinessRuleAccountTestDataMapping_ShouldReturnProperType_WhenConvertingToBusinessType()
        {
            var testData = new BusinessRuleAccountTestData
            {
                BusinessRuleAccountTestDataId = 1,
                ExternalId = Guid.NewGuid(),
                JsonData = RandomData.RandomString(10)
            };

            var actual = Sut.Map<BusinessRuleAccountTestData, BusinessEntities.BusinessRules.BusinessRuleAccountTestData>(testData);

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
                        BusinessRuleId = 1,
                        ExternalId = Guid.NewGuid(),
                        Name = RandomData.RandomString(10),
                        Type = "System.Int32"
                    }
                },
                AccountTestData = new BusinessRuleAccountTestData
                {
                    BusinessRuleAccountTestDataId = 1,
                    ExternalId = Guid.NewGuid(),
                    JsonData = RandomData.RandomString(10)
                }
            };


            var actual = Sut.Map<BusinessRuleDescription, BusinessEntities.BusinessRules.BusinessRuleDescription>(rule);

            Assert.AreEqual(rule.Group, actual.Group);
            Assert.AreEqual(rule.ExternalId, actual.ExternalId);
            Assert.AreEqual(rule.Script, actual.Script);
            Assert.AreEqual(rule.Parameters.Count, actual.Parameters.Count());
            Assert.AreEqual(rule.AccountTestData.ExternalId, actual.AccountTestData.ExternalId);
        }

        [Test]
        public void BusinessRuleDescriptionMapping_ShouldReturnBusinessRuleView_WhenConvertingToBusinessType()
        {
            var rule = new BusinessRuleDescription
            {
                Script = "012345678901234567890123456789012345678901234567890123456789",
                Group = RandomData.RandomEnum<BusinessRuleGroup>(),
                Name = RandomData.RandomString(10),
                Description = RandomData.RandomString(10),
                ExternalId = Guid.NewGuid(),
                Parameters = new List<BusinessRuleParameter>
                {
                    new BusinessRuleParameter
                    {
                        BusinessRuleId = 1,
                        ExternalId = Guid.NewGuid(),
                        Name = RandomData.RandomString(10),
                        Type = "System.Int32"
                    }
                },
                AccountTestData = new BusinessRuleAccountTestData
                {
                    BusinessRuleAccountTestDataId = 1,
                    ExternalId = Guid.NewGuid(),
                    JsonData = RandomData.RandomString(10)
                }
            };


            var actual = Sut.Map<BusinessRuleDescription, BusinessEntities.BusinessRules.BusinessRuleView>(rule);

            Assert.AreEqual(rule.Group, actual.Group);
            Assert.AreEqual(rule.ExternalId, actual.ExternalId);
            Assert.AreEqual("012345678901234567890123456789 ...", actual.CodeBeggining);
            Assert.AreEqual(rule.Name, actual.Name);
            Assert.AreEqual(rule.Description, actual.Description);
        }

        [Test]
        public void ProductRequestType_ShouldReturnProperType_WhenConvertingToBusinessType()
        {
            var mapping = new ProductRequestType
            {
                ProductRequestTypeId = RandomData.RandomInt(1, int.MaxValue),
                Name = RandomData.RandomString(100),
                ExternalId = Guid.NewGuid(),
                RequestGroups = new[]
                {
                    new ProductRequestGroup
                    {
                        ProductRequestGroupId = RandomData.RandomInt(1, int.MaxValue),
                        ExternalId = Guid.NewGuid(),
                        Name = RandomData.RandomString(1, 100),
                        ProductRequestTypeId = RandomData.RandomInt(1, int.MaxValue),
                    }
                }
            };

            var actual = Sut.Map<ProductRequestType, BusinessEntities.ProductRequests.ProductRequestType>(mapping);

            Assert.AreEqual(mapping.Name, actual.Name);
            Assert.AreEqual(mapping.ExternalId, actual.ExternalId);
            Assert.AreEqual(mapping.RequestGroups.Count, actual.RequestGroups.Count());
            Assert.AreEqual(mapping.RequestGroups.First().ExternalId, actual.RequestGroups.First().ExternalId);
            Assert.AreEqual(mapping.RequestGroups.First().Name, actual.RequestGroups.First().Name);
        }

        [Test]
        public void ProductRequestGroup_WhenConvertingToBusinessType()
        {
            var mapping = new ProductRequestGroup
            {
                ProductRequestTypeId = RandomData.RandomInt(1, int.MaxValue),
                Name = RandomData.RandomString(100),
                ExternalId = Guid.NewGuid(),
                RequestTasks = null,
                RequestType = Builder<ProductRequestType>.CreateNew()
                    .With(o => o.ExternalId, Guid.NewGuid())
                    .Build()
            };

            var actual = Sut.Map<ProductRequestGroup, BusinessEntities.ProductRequests.ProductRequestGroup>(mapping);

            Assert.AreEqual(mapping.Name, actual.Name);
            Assert.AreEqual(mapping.ExternalId, actual.ExternalId);
            Assert.AreEqual(mapping.RequestType.ExternalId, actual.ProductRequestTypeId);
            Assert.IsFalse(actual.RequestTasks.Any());
        }

        [Test]
        public void ProductRequestTask_WhenConvertingToBusinessType()
        {
            var mapping = new ProductRequestTask
            {
                ProductRequestTaskId = RandomData.RandomInt(1, int.MaxValue),
                Question = RandomData.RandomString(100),
                ExternalId = Guid.NewGuid(),
                RequestGroup = new ProductRequestGroup
                {
                    ExternalId = Guid.NewGuid(),
                }
            };

            var actual = Sut.Map<ProductRequestTask, BusinessEntities.ProductRequests.ProductRequestTask>(mapping);

            Assert.AreEqual(mapping.Question, actual.Question);
            Assert.AreEqual(mapping.ExternalId, actual.ExternalId);
            Assert.AreEqual(mapping.RequestGroup.ExternalId, actual.ProductRequestGroupId);
        }

        [Test]
        public void ProductRequestGroupAssignee_WhenConvertingToBusinessType()
        {
            var mapping = new ProductRequestGroupAssignee
            {
                ProductRequestGroupAssigneeId = RandomData.RandomInt(1, int.MaxValue),
                AccountId = RandomData.RandomInt(1, int.MaxValue),
                RequestGroup = new ProductRequestGroup
                {
                    ExternalId = Guid.NewGuid(),
                },
                Account = new DataAccount
                {
                    ExternalId = Guid.NewGuid(),
                    FullName = RandomData.RandomString(1, 100),
                    Email = RandomData.RandomEmail()
                }
            };

            var actual = Sut.Map<ProductRequestGroupAssignee, BusinessEntities.Auth.Account>(mapping);

            Assert.AreEqual(mapping.Account.ExternalId, actual.ExternalId);
            Assert.AreEqual(mapping.Account.FullName, actual.FullName);
            Assert.AreEqual(mapping.Account.Email, actual.Email);
        }

        [Test]
        public void ProductRequestRegistration()
        {
            var mapping = new ProductRequestRegistration
            {
                Description = RandomData.RandomString(1, 1024),
                ExternalId = Guid.NewGuid(),
                ProductRequestTypeId = RandomData.RandomInt(1, int.MaxValue),
                ProductRequestType = new ProductRequestType { ExternalId = Guid.NewGuid() },
                CreatedBy = new DataAccount { ExternalId = Guid.NewGuid() },
                CreatedOn = DateTime.UtcNow,
                Tasks = Builder<ProductRequestRegistrationTask>.CreateListOfSize(RandomData.RandomInt(1, 5))
                    .All()
                    .With(o => o.ProductRequestTask, Builder<ProductRequestTask>.CreateNew().With(x => x.ExternalId, Guid.NewGuid()).Build())
                    .With(o => o.IsCompleted, false)
                    .Build()
            };

            var actual = Sut.Map<ProductRequestRegistration, BusinessEntities.ProductRequests.ProductRequestRegistration>(mapping);

            Assert.AreEqual(mapping.ExternalId, actual.ExternalId);
            Assert.AreEqual(EnumDescriptionHelper.GetDescription(ProductRequestRegistrationStatus.New), actual.Status);
            Assert.AreEqual(mapping.Description, actual.Description);
            Assert.AreEqual(mapping.CreatedBy.ExternalId, actual.CreatedByAccountId);
            Assert.AreEqual(mapping.CreatedOn.ToUniversalTime(), actual.CreatedOn.ToUniversalTime(), "created on");
            Assert.AreEqual(DateTimeKind.Local, actual.CreatedOn.Kind);
            Assert.AreEqual(mapping.ProductRequestType.ExternalId, actual.ProductRequestTypeId);
            Assert.AreEqual(mapping.Tasks.Count, actual.Tasks.Count(), "tasks");

            foreach (var task in mapping.Tasks)
            {
                Assert.IsTrue(actual.Tasks.Any(o => o.ProductRequestTaskId == task.ProductRequestTask.ExternalId), "task not mapped");
            }
        }

        [Test]
        public void ProductRequestRegistrationTask()
        {
            var mapping = new ProductRequestRegistrationTask
            {
                IsCompleted = RandomData.RandomBool(),
                ProductRequestTask = new ProductRequestTask { ExternalId = Guid.NewGuid() },
                LastChangedByAccountId = RandomData.RandomInt(1, int.MaxValue),
                ProductRequestRegistration = new ProductRequestRegistration { ExternalId = Guid.NewGuid() },
                LastChangedBy = new DataAccount { ExternalId = Guid.NewGuid() },
                LastChangedOn = DateTime.UtcNow
            };

            var actual = Sut.Map<ProductRequestRegistrationTask, BusinessEntities.ProductRequests.ProductRequestRegistrationTask>(mapping);

            Assert.AreEqual(mapping.IsCompleted, actual.IsCompleted);
            Assert.AreEqual(mapping.ProductRequestTask.ExternalId, actual.ProductRequestTaskId);
            Assert.AreEqual(mapping.LastChangedBy.ExternalId, actual.LastChangedByAccountId);
            Assert.AreEqual(mapping.LastChangedOn.Value.ToUniversalTime(), actual.LastChangedOn.Value.ToUniversalTime());
            Assert.AreEqual(DateTimeKind.Local, actual.LastChangedOn.Value.Kind);
        }

        [Test]
        public void PluginConfiguration_To_GlobalPluginConfiguration_ShouldConvert_WhenAllPropertiesFilledOut()
        {
            var mapping = new PluginConfiguration
            {
                ExternalId = Guid.NewGuid(),
                PluginId = 1234,
                PluginConfigurationId = RandomData.RandomInt(int.MaxValue),
                PluginType = RandomData.RandomEnum<PluginType>(),
                Plugin = new DataEntities.Plugins.Plugin { ExternalId = Guid.NewGuid() }
            };

            var actual = Sut.Map<PluginConfiguration, BusinessEntities.Plugins.GlobalPluginConfiguration>(mapping);

            Assert.AreEqual(mapping.ExternalId, actual.ExternalId);
            Assert.AreEqual(mapping.PluginType, actual.PluginType);
            Assert.AreEqual(mapping.Plugin.ExternalId, actual.PluginId);
        }

        [Test]
        public void PluginConfiguration_To_GlobalPluginConfiguration_ShouldConvert_WhenPluginIsNotAssigned()
        {
            var mapping = new PluginConfiguration
            {
                ExternalId = Guid.NewGuid(),
                PluginId = null,
                PluginConfigurationId = RandomData.RandomInt(int.MaxValue),
                PluginType = RandomData.RandomEnum<PluginType>(),
                Plugin = null
            };

            var actual = Sut.Map<PluginConfiguration, BusinessEntities.Plugins.GlobalPluginConfiguration>(mapping);

            Assert.AreEqual(mapping.ExternalId, actual.ExternalId);
            Assert.AreEqual(mapping.PluginType, actual.PluginType);
            Assert.IsNull(actual.PluginId);
        }

        [Test]
        public void PackagePluginConfiguration_To_PackagePluginConfiguration_ShouldConvert_WhenAllPropertiesFilledOut()
        {
            var mapping = new PluginPackageConfiguration
            {
                ExternalId = Guid.NewGuid(),
                PluginId = RandomData.RandomInt(int.MaxValue),
                PluginPackageConfigurationId = RandomData.RandomInt(int.MaxValue),
                PluginType = RandomData.RandomEnum<PluginType>(),
                Plugin = new DataEntities.Plugins.Plugin { ExternalId = Guid.NewGuid() },
                PackageId = RandomData.RandomInt(int.MaxValue),
                Package = new Product
                {
                    ExternalId = Guid.NewGuid(),
                    Description = RandomData.RandomString(10),
                    BusinessUnit = new BusinessUnit { Description = RandomData.RandomString(10) }
                }
            };

            var actual = Sut.Map<PluginPackageConfiguration, BusinessEntities.Plugins.PackagePluginConfiguration>(mapping);

            Assert.AreEqual(mapping.ExternalId, actual.ExternalId);
            Assert.AreEqual(mapping.PluginType, actual.PluginType);
            Assert.AreEqual(mapping.Plugin.ExternalId, actual.PluginId);
            Assert.AreEqual(mapping.Package.ExternalId, actual.PackageId);
            Assert.AreEqual(mapping.Package.Description, actual.PackageName);
            Assert.AreEqual(mapping.Package.BusinessUnit.Description, actual.BusinessUnit);
        }

        [Test]
        public void PackagePluginConfiguration_To_PackagePluginConfiguration_ShouldConvert_WhenPluginIsNotAssigned()
        {
            var mapping = new PluginPackageConfiguration
            {
                ExternalId = Guid.NewGuid(),
                PluginId = null,
                PluginPackageConfigurationId = RandomData.RandomInt(int.MaxValue),
                PluginType = RandomData.RandomEnum<PluginType>(),
                Plugin = null,
                PackageId = RandomData.RandomInt(int.MaxValue),
                Package = new Product { ExternalId = Guid.NewGuid(), Description = RandomData.RandomString(10) }
            };

            var actual = Sut.Map<PluginPackageConfiguration, BusinessEntities.Plugins.PackagePluginConfiguration>(mapping);

            Assert.AreEqual(mapping.ExternalId, actual.ExternalId);
            Assert.AreEqual(mapping.PluginType, actual.PluginType);
            Assert.IsNull(actual.PluginId);
            Assert.AreEqual(mapping.Package.ExternalId, actual.PackageId);
            Assert.AreEqual(mapping.Package.Description, actual.PackageName);
        }

        [Test]
        public void Plugin_To_Plugin_ShouldConvert_WhenMapped()
        {
            var mapping = new DataEntities.Plugins.Plugin
            {
                ExternalId = Guid.NewGuid(),
                PluginId = RandomData.RandomInt(int.MaxValue),
                PluginType = PluginType.Authentication | PluginType.HelpDesk | PluginType.DeploymentTool,
                Key = RandomData.RandomString(10)
            };

            var actual = Sut.Map<DataEntities.Plugins.Plugin, BusinessEntities.Plugins.Plugin>(mapping);

            Assert.AreEqual(mapping.ExternalId, actual.PluginId);
            Assert.AreEqual(mapping.Key, actual.PluginKey);
            Assert.AreEqual(3, actual.PluginTypes.Count());
            Assert.IsTrue(actual.PluginTypes.Contains(PluginType.Authentication));
            Assert.IsTrue(actual.PluginTypes.Contains(PluginType.HelpDesk));
            Assert.IsTrue(actual.PluginTypes.Contains(PluginType.DeploymentTool));
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

            var result = Sut.Map<ReleaseJob, BusinessEntities.DeploymentTool.ReleaseJob>(releaseJob);

            Assert.AreEqual(releaseJob.ExternalId, result.ExternalId);
            Assert.AreEqual(releaseJob.IsIncluded, result.IsIncluded);
            Assert.AreEqual(releaseJob.JobId, result.JobId);
            Assert.AreEqual(releaseJob.Name, result.Name);
            Assert.AreEqual(releaseJob.Order, result.Order);
        }

        [Test]
        public void ReleaseTask_ShouldReturnDataReleaseTask_WhenMapFromBusinessEntity()
        {
            var releaseJob = new ReleaseTask
            {
                ReleaseWindow = new ReleaseWindow { ExternalId = Guid.NewGuid() },
                ExternalId = Guid.NewGuid(),
                CreatedBy = new DataAccount
                {
                    FullName = RandomData.RandomString(10),
                    ExternalId = Guid.NewGuid()
                },
                CreatedOn = RandomData.RandomDateTime(DateTimeKind.Utc),
                CompletedOn = RandomData.RandomDateTime(DateTimeKind.Utc),
                Assignee = new DataAccount
                {
                    FullName = RandomData.RandomString(10),
                    ExternalId = Guid.NewGuid()
                },
                HelpDeskReference = RandomData.RandomString(10),
                HelpDeskUrl = RandomData.RandomString(10)
            };

            var result = Sut.Map<ReleaseTask, BusinessEntities.ReleasePlan.ReleaseTask>(releaseJob);

            Assert.AreEqual(releaseJob.ReleaseWindow.ExternalId, result.ReleaseWindowId);
            Assert.AreEqual(releaseJob.ExternalId, result.ExternalId);
            Assert.AreEqual(releaseJob.CreatedBy.FullName, result.CreatedBy);
            Assert.AreEqual(releaseJob.CreatedBy.ExternalId, result.CreatedByExternalId);
            Assert.AreEqual(releaseJob.CreatedOn.ToLocalTime(), result.CreatedOn);
            Assert.AreEqual(releaseJob.CompletedOn.Value.ToLocalTime(), result.CompletedOn);
            Assert.AreEqual(releaseJob.Assignee.FullName, result.Assignee);
            Assert.AreEqual(releaseJob.HelpDeskReference, result.HelpDeskTicketReference);
            Assert.AreEqual(releaseJob.HelpDeskUrl, result.HelpDeskTicketUrl);
            Assert.AreEqual(releaseJob.Assignee.ExternalId, result.AssigneeExternalId);
        }

        #region Helpers

        private DataReleaseWindow CreateReleaseWindow()
        {
            return Builder<DataReleaseWindow>.CreateNew()
                        .With(o => o.ExternalId, Guid.NewGuid())
                        .With(o => o.Sprint = RandomData.RandomString(5))
                        .With(o => o.StartTime, RandomData.RandomDateTime(DateTimeKind.Utc))
                        .With(o => o.ReleaseType = RandomData.RandomEnum<DataReleaseType>())
                        .With(o => o.CreatedOn, RandomData.RandomDateTime(DateTimeKind.Utc))
                        .With(o => o.OriginalStartTime, RandomData.RandomDateTime(DateTimeKind.Utc))
                        .With(o => o.ReleaseWindowId, RandomData.RandomInt(int.MaxValue))
                        .With(o => o.RequiresDowntime, RandomData.RandomBool())
                        .With(o => o.Metrics, Builder<Metric>.CreateListOfSize(2)
                            .TheFirst(1)
                            .With(x => x.ExecutedOn, RandomData.RandomDateTime(DateTimeKind.Utc))
                            .With(x => x.MetricType, MetricType.Close)
                            .TheNext(1)
                            .With(x => x.ExecutedOn, RandomData.RandomDateTime(DateTimeKind.Utc))
                            .With(x => x.MetricType, MetricType.Approve)
                            .Build())
                        .With(o => o.ReleaseDecision, RandomData.RandomEnum<ReleaseDecision>())
                        .With(o => o.ReleaseProducts, Builder<ReleaseProduct>.CreateListOfSize(RandomData.RandomInt(3, 5))
                            .All()
                            .With(x => x.Product = Builder<DataProduct>.CreateNew().With(p => p.Description = RandomData.RandomString(5)).Build())
                            .Build())
                        .With(o => o.ReleaseNotes, Builder<DataReleaseNote>.CreateNew()
                            .With(x => x.ReleaseNotes, RandomData.RandomString(100))
                            .With(x => x.ReleaseNoteId, RandomData.RandomInt(int.MaxValue))
                            .With(x => x.Issues, RandomData.RandomString(100))
                            .Build())
                        .With(o => o.IsFailed, RandomData.RandomBool())
                        .Build();
        }
        private BusinessReleaseWindow CreateReleaseWindow(DataReleaseWindow releaseWindow)
        {
            return Builder<BusinessReleaseWindow>.CreateNew()
                        .With(o => o.ExternalId, releaseWindow.ExternalId)
                        .With(o => o.Products = releaseWindow.ReleaseProducts.Select(x => x.Product.Description).ToList())
                        .With(o => o.Sprint = releaseWindow.Sprint)
                        .With(o => o.StartTime, releaseWindow.StartTime.ToLocalTime())
                        .With(o => o.ReleaseType, releaseWindow.ReleaseType)
                        .With(o => o.OriginalStartTime, releaseWindow.OriginalStartTime.ToLocalTime())
                        .With(o => o.RequiresDowntime, releaseWindow.RequiresDowntime)
                        .With(o => o.ApprovedOn, releaseWindow.Metrics.IsNullOrEmpty() ? releaseWindow.Metrics.First(x => x.MetricType == MetricType.Approve).ExecutedOn : null)
                        .With(o => o.ClosedOn, releaseWindow.Metrics.IsNullOrEmpty() ? releaseWindow.Metrics.First(x => x.MetricType == MetricType.Close).ExecutedOn : null)
                        .With(o => o.ReleaseDecision, EnumDescriptionHelper.GetDescription(releaseWindow.ReleaseDecision))
                        .With(o => o.IsFailed, releaseWindow.IsFailed)
                        .Build();
        }

        private DataProduct CreateProduct()
        {
            return Builder<DataProduct>.CreateNew()
                        .With(o => o.ProductId = RandomData.RandomInt(1000))
                        .With(o => o.Description = RandomData.RandomString(5))
                        .With(o => o.ExternalId = Guid.NewGuid())
                        .With(o => o.BusinessUnit, new BusinessUnit { ExternalId = Guid.NewGuid() })
                        .Build();
        }

        private BusinessProduct CreateProduct(DataProduct product)
        {
            return Builder<BusinessProduct>.CreateNew()
                        .With(o => o.ExternalId = product.ExternalId)
                        .With(o => o.Description = product.Description)
                        .With(o => o.BusinessUnit = new BusinessBusinessUnit { ExternalId = product.BusinessUnit.ExternalId })
                        .Build();
        }

        private DataAccountProduct CreateAccountProduct()
        {
            var product = CreateProduct();

            return Builder<DataAccountProduct>.CreateNew()
                .With(o => o.AccountProductId, RandomData.RandomInt(500))
                .With(o => o.CreatedOn, RandomData.RandomDate())
                .With(o => o.IsDefault, RandomData.RandomBool())
                .With(o => o.Product, product)
                .With(o => o.ProductId, product.ProductId)
                .Build();
        }

        private DataAccount CreateAccount()
        {
            var acc = Builder<DataAccount>.CreateNew()
                        .With(o => o.AccountId, RandomData.RandomInt(500))
                        .With(o => o.ExternalId, Guid.NewGuid())
                        .With(o => o.CreatedOn, RandomData.RandomDate())
                        .With(o => o.Description, RandomData.RandomString(5))
                        .With(o => o.Email, RandomData.RandomEmail())
                        .With(o => o.FullName, RandomData.RandomString(10))
                        .With(o => o.IsBlocked, RandomData.RandomBool())
                        .With(o => o.Name, RandomData.RandomString(10))
                        .With(o => o.Role, new Role())
                        .With(o => o.AccountProducts, new List<DataAccountProduct> {
                            CreateAccountProduct(),
                            CreateAccountProduct()
                        })
                        .Build();
            return acc;
        }

        #endregion
    }
}
