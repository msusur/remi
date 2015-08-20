using System.Linq;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Queries.ReleaseParticipant;
using ReMi.QueryHandlers.ReleaseParticipant;
using System;
using System.Collections.Generic;
using ReMi.TestUtils.UnitTests;

namespace ReMi.QueryHandlers.Tests
{
    public class GetReleaseParticipantsHandlerTests : TestClassFor<GetReleaseParticipantsHandler>
    {
        private Mock<IReleaseParticipantGateway> _releaseParticipantGatewayMock;

        private readonly Account _account = new Account
        {
            Email = RandomData.RandomEmail(),
            FullName = RandomData.RandomString(10),
            Role = new Role { Name = new[] { "Admin", "BasicUser", "NotAuthenticated", "ProductOwner" }[RandomData.RandomInt(0, 3)] }
        };

        private BusinessEntities.ReleasePlan.ReleaseParticipant _releaseParticipant;
        private Guid _releaseWindowId;
        
        protected override GetReleaseParticipantsHandler ConstructSystemUnderTest()
        {
            return new GetReleaseParticipantsHandler
            {
                ReleaseParticipantGatewayFactory = () => _releaseParticipantGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseParticipant = new BusinessEntities.ReleasePlan.ReleaseParticipant
            {
                Account = _account,
                ReleaseWindowId = _releaseWindowId,
                ReleaseParticipantId = Guid.NewGuid()
            };

            _releaseParticipantGatewayMock = new Mock<IReleaseParticipantGateway>();

            _releaseWindowId = Guid.NewGuid();
            _releaseParticipantGatewayMock.Setup(rp => rp.GetReleaseParticipants(_releaseWindowId))
                .Returns(new List<BusinessEntities.ReleasePlan.ReleaseParticipant>
                {
                    new BusinessEntities.ReleasePlan.ReleaseParticipant {Account = _account}
                });

            base.TestInitialize();
        }

        [Test]
        public void GetReleaseParpicipants_ShouldReturnCorrectList()
        {
            var result = Sut.Handle(new GetReleaseParticipantRequest { ReleaseWindowId = _releaseWindowId });

            Assert.AreEqual(1, result.Participants.Count());
        }

        [Test]
        public void GetReleaseParpicipants_ShouldReturnCorrectEmail()
        {
            var result = Sut.Handle(new GetReleaseParticipantRequest { ReleaseWindowId = _releaseWindowId });

            Assert.AreEqual(_account.Email, result.Participants.First().Account.Email);
        }

        [Test]
        public void GetReleaseParpicipants_ShouldReturnCorrectFullName()
        {
            var result = Sut.Handle(new GetReleaseParticipantRequest { ReleaseWindowId = _releaseWindowId });

            Assert.AreEqual(_account.FullName, result.Participants.First().Account.FullName);
        }

        [Test]
        public void GetReleaseParpicipants_ShouldReturnCorrectRole()
        {
            var result = Sut.Handle(new GetReleaseParticipantRequest { ReleaseWindowId = _releaseWindowId });

            Assert.AreEqual(_account.Role, result.Participants.First().Account.Role);
        }

        [Test]
        public void GetReleaseParpicipants_ShouldCallGatewayCorrectly()
        {
            Sut.Handle(new GetReleaseParticipantRequest { ReleaseWindowId = _releaseWindowId });

            _releaseParticipantGatewayMock.Verify(rp => rp.GetReleaseParticipants(_releaseWindowId));
        }
    }
}
