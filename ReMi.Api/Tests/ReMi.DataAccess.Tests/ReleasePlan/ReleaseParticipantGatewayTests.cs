using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Moq;
using NUnit.Framework;
using ReMi.Common.Utils.Repository;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.DataEntities;
using ReMi.DataEntities.Auth;
using ReMi.DataEntities.ReleaseCalendar;
using ReMi.DataEntities.ReleaseExecution;
using ReMi.DataEntities.ReleasePlan;

namespace ReMi.DataAccess.Tests.ReleasePlan
{
    public class ReleaseParticipantGatewayTests : TestClassFor<ReleaseParticipantGateway>
    {
        private Mock<IRepository<ReleaseWindow>> _releaseWindowRepositoryMock;
        private Mock<IRepository<Account>> _accountRepositoryMock;
        private Mock<IRepository<ReleaseParticipant>> _releaseParticipantRepositoryMock;
        private Mock<IMappingEngine> _mappingEngineMock;

        protected override ReleaseParticipantGateway ConstructSystemUnderTest()
        {
            return new ReleaseParticipantGateway
            {
                ReleaseWindowRepository = _releaseWindowRepositoryMock.Object,
                AccountRepository = _accountRepositoryMock.Object,
                ReleaseParticipantRepository = _releaseParticipantRepositoryMock.Object,
                Mapper = _mappingEngineMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseWindowRepositoryMock = new Mock<IRepository<ReleaseWindow>>();
            _accountRepositoryMock = new Mock<IRepository<Account>>();
            _releaseParticipantRepositoryMock = new Mock<IRepository<ReleaseParticipant>>();
            _mappingEngineMock = new Mock<IMappingEngine>();
            base.TestInitialize();
        }

        [Test]
        public void GetReleaseMembers_ShouldCallMapperWithCorrectParameters()
        {
            var window = new ReleaseWindow
            {
                ExternalId = Guid.NewGuid(),
                ReleaseApprovers =
                    new List<ReleaseApprover>
                    {
                        new ReleaseApprover {Account = new Account {Email = RandomData.RandomEmail()}}
                    },
                SignOffs = new List<SignOff> {new SignOff {Account = new Account {Email = RandomData.RandomEmail()}}},
                ReleaseParticipants =
                    new List<ReleaseParticipant>
                    {
                        new ReleaseParticipant {Account = new Account {Email = RandomData.RandomEmail()}}
                    }
            };

            _releaseWindowRepositoryMock.SetupEntities(new[] {window});

            Sut.GetReleaseMembers(window.ExternalId);

            _mappingEngineMock.Verify(
                x =>
                    x.Map<List<Account>, List<BusinessEntities.Auth.Account>>(
                        It.Is<List<Account>>(
                            l =>
                                l.Any(rp => rp.Email == window.ReleaseParticipants.ToList()[0].Account.Email) &&
                                l.Any(rp => rp.Email == window.ReleaseApprovers.ToList()[0].Account.Email) &&
                                l.Any(rp => rp.Email == window.SignOffs.ToList()[0].Account.Email))));
        }
    }
}
