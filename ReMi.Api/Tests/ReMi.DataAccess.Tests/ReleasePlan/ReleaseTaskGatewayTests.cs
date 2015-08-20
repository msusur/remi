using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Common.Constants;
using ReMi.Common.Constants.ReleasePlan;
using ReMi.Common.Utils;
using ReMi.DataAccess.AutoMapper;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.Auth;
using ReMi.DataEntities.ReleaseCalendar;
using ReMi.DataEntities.ReleasePlan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ReMi.Common.Utils.Enums;
using ReMi.Common.Utils.Repository;
using ReMi.TestUtils.UnitTests;
using ReleaseTask = ReMi.BusinessEntities.ReleasePlan.ReleaseTask;
using ReleaseTaskAttachment = ReMi.DataEntities.ReleasePlan.ReleaseTaskAttachment;
using ReleaseTaskDto = ReMi.DataEntities.ReleasePlan.ReleaseTask;

namespace ReMi.DataAccess.Tests.ReleasePlan
{
    public class ReleaseTaskGatewayTests : TestClassFor<ReleaseTaskGateway>
    {
        private Mock<IRepository<ReleaseWindow>> _releaseWindowRepositoryMock;
        private Mock<IRepository<ReleaseTaskDto>> _releaseTaskRepositoryMock;
        private Mock<IRepository<ReleaseTaskAttachment>> _releaseTaskAttachmentRepositoryMock;
        private Mock<IRepository<ReleaseTaskTypeDescription>> _releaseTaskTypeRepositoryMock;
        private Mock<IRepository<Account>> _accountRepositoryMock;

        [TestFixtureSetUp]
        public static void AutoMapperInitialize()
        {
            Mapper.Initialize(
                c =>
                {
                    c.AddProfile<BusinessEntityToDataEntityMappingProfile>();
                    c.AddProfile(new DataEntityToBusinessEntityMappingProfile());
                });
        }

        protected override ReleaseTaskGateway ConstructSystemUnderTest()
        {
            return new ReleaseTaskGateway
            {
                ReleaseWindowRepository = _releaseWindowRepositoryMock.Object,
                ReleaseTaskRepository = _releaseTaskRepositoryMock.Object,
                ReleaseTaskAttachmentRepository = _releaseTaskAttachmentRepositoryMock.Object,
                ReleaseTaskTypeRepository = _releaseTaskTypeRepositoryMock.Object,
                AccountRepository = _accountRepositoryMock.Object,
                Mapper = Mapper.Engine
            };
        }

        protected override void TestInitialize()
        {
            _releaseWindowRepositoryMock = new Mock<IRepository<ReleaseWindow>>();
            _releaseTaskRepositoryMock = new Mock<IRepository<ReleaseTaskDto>>();
            _releaseTaskAttachmentRepositoryMock = new Mock<IRepository<ReleaseTaskAttachment>>();
            _releaseTaskTypeRepositoryMock = new Mock<IRepository<ReleaseTaskTypeDescription>>();
            _accountRepositoryMock = new Mock<IRepository<Account>>();

            base.TestInitialize();
        }

        [Test]
        public void GetRelaseTasks_ShoudGetReleaseTasksList_WhenReleaseWindowExists()
        {
            var releaseWindow = SetupRelaseWindows();
            var expected = BuildRelaseTaskViews(releaseWindow.ReleaseTasks);

            var actual = Sut.GetReleaseTaskViews(releaseWindow.ExternalId);

            CollectionAssertHelper.AreEqual<ReleaseTaskView>(expected, actual, (e, a) =>
                    e.CreatedBy == a.CreatedBy
                    && e.Description == a.Description
                    && e.ExternalId == a.ExternalId
                    && e.Type == a.Type);
        }

        [Test]
        public void GetRelaseTask_ShoudGetReleaseTask_WhenReleaseTaskExists()
        {
            var releaseWindow = SetupRelaseWindows();
            var expected = BuildRelaseTaskViews(releaseWindow.ReleaseTasks).First();

            var actual = Sut.GetReleaseTaskView(releaseWindow.ReleaseTasks.First().ExternalId);

            Assert.AreEqual(expected.ExternalId, actual.ExternalId);
            Assert.AreEqual(expected.Description, actual.Description);
        }

        [Test]
        public void GetReleaseTaskAttachment_ShouldGetReleaseTaskAttachmentContent_WhenAttachmentExternalIdExists()
        {
            var releaseWindow = SetupRelaseWindows();
            var externalId = releaseWindow.ReleaseTasks.First().Attachments.First().ExternalId;
            var expected = releaseWindow.ReleaseTasks.First().Attachments.First();

            var actual = Sut.GetReleaseTaskAttachment(externalId);

            CollectionAssert.AreEqual(expected.Attachment, actual.Attachment);
            Assert.AreEqual(expected.ExternalId, actual.ExternalId);
            Assert.AreEqual(expected.HelpDeskAttachmentId, actual.HelpDeskAttachmentId);
            Assert.AreEqual(expected.Name, actual.Name);
        }

        [Test]
        public void GetHelpDeskTicketReference_ShouldGetHelpDeskReference_WhenReleaseTaskExists()
        {
            var releaseWindow = SetupRelaseWindows();
            var externalId = releaseWindow.ReleaseTasks.First().ExternalId;
            var expected = releaseWindow.ReleaseTasks.First().HelpDeskReference;

            var actual = Sut.GetHelpDeskTicketReference(externalId);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CompleteTask_ShouldUpdateRepository()
        {
            var releaseTaskId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            _releaseTaskRepositoryMock.SetupEntities(new List<ReleaseTaskDto>
            {
                new ReleaseTaskDto {ExternalId = releaseTaskId}
            });
            var account = new Account { ExternalId = accountId, AccountId = RandomData.RandomInt(322) };
            _accountRepositoryMock.SetupEntities(new List<Account> { account });

            Sut.CompleteTask(releaseTaskId, accountId);

            _releaseTaskRepositoryMock.Verify(
                r =>
                    r.Update(
                        It.Is<ReleaseTaskDto>(
                            t => t.AssigneeAccountId == account.AccountId && t.ExternalId == releaseTaskId)));
        }

        [Test]
        [ExpectedException(typeof(ReleaseTaskAlreadyCompletedException))]
        public void CompleteTask_ShouldThrowTaskAlreadyCompletedException()
        {
            var releaseTaskId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            _releaseTaskRepositoryMock.SetupEntities(new List<ReleaseTaskDto>
            {
                new ReleaseTaskDto {ExternalId = releaseTaskId, CompletedOn = SystemTime.Now.AddMinutes(-2)}
            });

            Sut.CompleteTask(releaseTaskId, accountId);
        }

        [Test]
        public void CreateReleaseTask_ShouldCreateReleaseTaskWithProperOrder_WhenCalled()
        {
            var releaseWindow = SetupRelaseWindows();
            var dateTimeNow = RandomData.RandomDateTime(DateTimeKind.Utc);
            var account = new Account { ExternalId = Guid.NewGuid(), AccountId = RandomData.RandomInt(322) };
            var releaseTask = BuildReleaseTask(releaseWindow.ExternalId, account.ExternalId);

            _releaseWindowRepositoryMock.SetupEntities(new[] { releaseWindow });
            _accountRepositoryMock.SetupEntities(new[] { account });
            SystemTime.Mock(dateTimeNow);

            Sut.CreateReleaseTask(releaseTask);

            _releaseTaskRepositoryMock.Verify(x => x.Insert(It.Is<ReleaseTaskDto>(
                entity => entity.CreatedOn == dateTimeNow
                    && entity.ReleaseWindowId == releaseWindow.ReleaseWindowId
                    && entity.Order == 4)),
                Times.Once());
        }

        [Test]
        public void CreateReleaseTask_ShouldCreateReleaseTaskWithProperOrder_WhenCreatingFirstTaskInRelease()
        {
            var releaseWindow = SetupRelaseWindows();
            releaseWindow.ReleaseTasks.Clear();
            var dateTimeNow = RandomData.RandomDateTime(DateTimeKind.Utc);
            var account = new Account { ExternalId = Guid.NewGuid(), AccountId = RandomData.RandomInt(322) };
            var releaseTask = BuildReleaseTask(releaseWindow.ExternalId, account.ExternalId);

            _releaseWindowRepositoryMock.SetupEntities(new[] { releaseWindow });
            _accountRepositoryMock.SetupEntities(new[] { account });
            SystemTime.Mock(dateTimeNow);

            Sut.CreateReleaseTask(releaseTask);

            _releaseTaskRepositoryMock.Verify(x => x.Insert(It.Is<ReleaseTaskDto>(
                entity => entity.CreatedOn == dateTimeNow
                    && entity.ReleaseWindowId == releaseWindow.ReleaseWindowId
                    && entity.Order == 1)),
                Times.Once());
        }

        [Test]
        [ExpectedException(typeof(ReleaseWindowNotFoundException))]
        public void CreateReleaseTask_ShouldThrowException_WhenReleaseWindowNotFound()
        {
            var releaseWindow = SetupRelaseWindows();
            var releaseTask = BuildReleaseTask(Guid.NewGuid(), Guid.NewGuid());

            _releaseWindowRepositoryMock.SetupEntities(new[] { releaseWindow });

            Sut.CreateReleaseTask(releaseTask);
        }

        [Test]
        [ExpectedException(typeof(AccountIsBlockedException))]
        public void CreateReleaseTask_ShouldThrowException_WhenAssigneeAccountIsBlocked()
        {
            var releaseWindow = SetupRelaseWindows();
            var dateTimeNow = RandomData.RandomDateTime(DateTimeKind.Utc);
            var account = new Account { ExternalId = Guid.NewGuid(), AccountId = RandomData.RandomInt(322), IsBlocked = true };
            var releaseTask = BuildReleaseTask(releaseWindow.ExternalId, account.ExternalId);

            _releaseWindowRepositoryMock.SetupEntities(new[] { releaseWindow });
            _accountRepositoryMock.SetupEntities(new[] { account });
            SystemTime.Mock(dateTimeNow);

            Sut.CreateReleaseTask(releaseTask);
        }

        [Test]
        public void UpdateReleaseTasksOrder_ShouldUpdateTasksOrder_WhenCalled()
        {
            var releaseTasks = SetupRelaseWindows().ReleaseTasks;

            _releaseTaskRepositoryMock.SetupEntities(releaseTasks);

            Sut.UpdateReleaseTasksOrder(new Dictionary<Guid, short>
            {
                { releaseTasks.First().ExternalId, 2 },
                { releaseTasks.ElementAt(1).ExternalId, 3 },
                { releaseTasks.Last().ExternalId, 1 }
            });

            _releaseTaskRepositoryMock.Verify(x => x.Update(It.IsAny<Expression<Func<ReleaseTaskDto, bool>>>(), It.IsAny<Action<ReleaseTaskDto>>()),
                Times.Exactly(3));
        }

        private ReleaseWindow SetupRelaseWindows()
        {
            var releaseWindow = Builder<ReleaseWindow>.CreateNew()
                .With(x => x.ExternalId = Guid.NewGuid())
                .Build();

            BuildRelaseTasksDto(releaseWindow);

            var releaseTaskTypes = EnumDescriptionHelper.GetEnumDescriptions<ReleaseTaskType, ReleaseTaskTypeDescription>();

            _releaseWindowRepositoryMock.SetupEntities(new[] { releaseWindow });

            _releaseTaskRepositoryMock.SetupEntities(releaseWindow.ReleaseTasks);

            _releaseTaskAttachmentRepositoryMock.SetupEntities(releaseWindow.ReleaseTasks.First().Attachments);

            _releaseTaskTypeRepositoryMock.SetupEntities(releaseTaskTypes);

            return releaseWindow;
        }

        private static void BuildRelaseTasksDto(ReleaseWindow releaseWindow)
        {
            releaseWindow.ReleaseTasks = new List<ReleaseTaskDto>
            {
                new ReleaseTaskDto
                {
                    ReleaseWindow = releaseWindow,
                    ExternalId = Guid.NewGuid(),
                    Type = ReleaseTaskType.PreRelease,
                    Description = "Description 1",
                    CreatedByAccountId = RandomData.RandomInt(1, int.MaxValue),
                    CreatedBy = Builder<Account>.CreateNew().Build(),
                    HelpDeskReference = RandomData.RandomString(10),
                    HelpDeskUrl = RandomData.RandomString(1, 255),
                    Order = 1,
                    Attachments = new List<ReleaseTaskAttachment>
                    {
                        new ReleaseTaskAttachment
                        {
                            Attachment = new byte[0],
                            ExternalId = Guid.NewGuid(),
                            HelpDeskAttachmentId = RandomData.RandomString(10),
                            Name = "Attachment 1"
                        }
                    }
                },
                new ReleaseTaskDto
                {
                    ReleaseWindow = releaseWindow,
                    ExternalId = Guid.NewGuid(),
                    Type = ReleaseTaskType.PostRelease,
                    Description = "Description 2",
                    CreatedBy = Builder<Account>.CreateNew().Build(),
                    CreatedByAccountId = RandomData.RandomInt(1, int.MaxValue),
                    Order = 2,
                },
                new ReleaseTaskDto
                {
                    ReleaseWindow = releaseWindow,
                    ExternalId = Guid.NewGuid(),
                    Type = ReleaseTaskType.PostRelease,
                    Description = "Description 3",
                    CreatedBy = Builder<Account>.CreateNew().Build(),
                    CreatedByAccountId = RandomData.RandomInt(1, int.MaxValue),
                    Order = 3,
                }
            };
        }

        private static ReleaseTask BuildReleaseTask(Guid releaseWindowId, Guid accountId)
        {
            return Builder<ReleaseTask>.CreateNew()
                .With(x => x.ReleaseWindowId, releaseWindowId)
                .With(x => x.AssigneeExternalId, accountId)
                .With(x => x.CreatedByExternalId, accountId)
                .With(x => x.ExternalId, Guid.NewGuid())
                .With(x => x.Description, RandomData.RandomString(10))
                .With(x => x.Type, RandomData.RandomEnum<ReleaseTaskType>().ToString())
                .With(x => x.Risk, RandomData.RandomEnum<ReleaseTaskRisk>().ToString())
                .With(x => x.WhereTested, RandomData.RandomEnum<ReleaseTaskEnvironment>().ToString())
                .Build();
        }

        private static IEnumerable<ReleaseTaskView> BuildRelaseTaskViews(IEnumerable<ReleaseTaskDto> releaseTasks)
        {
            return releaseTasks.Select(
                x => new ReleaseTaskView
                {
                    CreatedBy = x.CreatedBy.FullName,
                    Description = x.Description,
                    ExternalId = x.ExternalId,
                    Type = x.Type.ToString(),
                    HelpDeskReference = x.HelpDeskReference
                });
        }

    }
}
