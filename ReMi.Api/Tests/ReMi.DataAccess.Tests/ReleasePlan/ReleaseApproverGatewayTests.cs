using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.Common.Utils.Repository;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.Auth;
using ReMi.DataEntities.ReleaseCalendar;
using System;
using System.Linq;
using ReleaseApprover = ReMi.DataEntities.ReleasePlan.ReleaseApprover;

namespace ReMi.DataAccess.Tests.ReleasePlan
{
    public class ReleaseApproverGatewayTests : TestClassFor<ReleaseApproverGateway>
    {
        private Mock<IRepository<ReleaseWindow>> _releaseWindowRepositoryMock;
        private Mock<IRepository<ReleaseApprover>> _releaseApproverRepositoryMock;
        private Mock<IRepository<Account>> _accountRepositoryMock;

        protected override ReleaseApproverGateway ConstructSystemUnderTest()
        {
            return new ReleaseApproverGateway
            {
                ReleaseWindowRepository = _releaseWindowRepositoryMock.Object,
                ReleaseApproverRepository = _releaseApproverRepositoryMock.Object,
                AccountRepository = _accountRepositoryMock.Object,
                MappingEngine = Mapper.Engine
            };
        }

        protected override void TestInitialize()
        {
            _releaseWindowRepositoryMock = new Mock<IRepository<ReleaseWindow>>();
            _releaseApproverRepositoryMock = new Mock<IRepository<ReleaseApprover>>();
            _accountRepositoryMock = new Mock<IRepository<Account>>();

            base.TestInitialize();
        }

        [Test]
        public void ClearApproverSignatures_ShouldClearAllSignaturesFromApproversList_WhenReleaseWindowFound()
        {
            var releaseWindow = Builder<ReleaseWindow>.CreateNew()
                .With(x => x.ExternalId = Guid.NewGuid())
                .With(x => x.ReleaseApprovers, Builder<ReleaseApprover>.CreateListOfSize(5)
                    .All()
                    .Do(a => a.ApprovedOn = RandomData.RandomDateTime(DateTimeKind.Utc))
                    .Build())
                .Build();

            _releaseWindowRepositoryMock.SetupEntities(new[] { releaseWindow });
            _releaseWindowRepositoryMock.Setup(x => x.Update(releaseWindow))
                .Returns((ChangedFields<ReleaseWindow>)null);

            Sut.ClearApproverSignatures(releaseWindow.ExternalId);

            _releaseWindowRepositoryMock.Verify(x => x.Update(It.IsAny<ReleaseWindow>()), Times.Once);
            Assert.IsFalse(releaseWindow.ReleaseApprovers.Any(x => x.ApprovedOn.HasValue), "All ApprovedOn properties should be cleared out");
        }

        [Test]
        public void ClearApproverSignatures_ShouldThrowException_WhenReleaseWindowNotFound()
        {
            var releaseWindowId = Guid.NewGuid();
            _releaseWindowRepositoryMock.SetupEntities(Enumerable.Empty<ReleaseWindow>());

            var ex = Assert.Throws<EntityNotFoundException>(() => Sut.ClearApproverSignatures(releaseWindowId));

            Assert.IsTrue(ex.Message.Contains(releaseWindowId.ToString()));
            _releaseWindowRepositoryMock.Verify(x => x.Update(It.IsAny<ReleaseWindow>()), Times.Never);
        }
    }
}
