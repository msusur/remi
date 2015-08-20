using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Common.Constants.ReleasePlan;
using ReMi.Common.Utils.Repository;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.DataEntities.ReleaseCalendar;
using ReMi.DataEntities.ReleasePlan;

namespace ReMi.DataAccess.Tests.ReleasePlan
{
    [TestFixture]
    public class ReleaseContentGatewayTests : TestClassFor<ReleaseContentGateway>
    {
        private Mock<IRepository<ReleaseContent>> _releaseContentRepositoryMock;
        private Mock<IRepository<DataEntities.Auth.Account>> _accountRepositoryMock;
        private Mock<IRepository<ReleaseWindow>> _releaseWindowRepositoryMock;
        private Mock<IMappingEngine> _mappingEngineMock;

        protected override ReleaseContentGateway ConstructSystemUnderTest()
        {
            return new ReleaseContentGateway
            {
                ReleaseContentRepository = _releaseContentRepositoryMock.Object,
                Mapper = _mappingEngineMock.Object,
                AccountRepository = _accountRepositoryMock.Object,
                ReleaseWindowRepository = _releaseWindowRepositoryMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseContentRepositoryMock = new Mock<IRepository<ReleaseContent>>(MockBehavior.Strict);
            _accountRepositoryMock = new Mock<IRepository<DataEntities.Auth.Account>>(MockBehavior.Strict);
            _mappingEngineMock = new Mock<IMappingEngine>(MockBehavior.Strict);
            _releaseWindowRepositoryMock = new Mock<IRepository<ReleaseWindow>>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void GetTicketInformations_ShouldReturnListOfTicket_WhenMatchSomething()
        {
            var tickets = new List<ReleaseContent>
            {
                new ReleaseContent {TicketId = new Guid(1, 0, 0, new byte[8])},
                new ReleaseContent {TicketId = new Guid(2, 0, 0, new byte[8])},
                new ReleaseContent {TicketId = new Guid(3, 0, 0, new byte[8])}
            };
            _releaseContentRepositoryMock.SetupEntities(tickets);

            _mappingEngineMock.Setup(x => x.Map<ReleaseContent, ReleaseContentTicket>(It.IsAny<ReleaseContent>()))
                .Returns(new ReleaseContentTicket());

            var actual = Sut.GetTicketInformations(new[]
            {
                new Guid(1, 0, 0, new byte[8]),
                new Guid(4, 0, 0, new byte[8]),
                new Guid(5, 0, 0, new byte[8]),
                new Guid(3, 0, 0, new byte[8]),
            });

            Assert.AreEqual(2, actual.Count());

            _mappingEngineMock.Verify(x => x.Map<ReleaseContent, ReleaseContentTicket>(It.Is<ReleaseContent>(y => y.TicketId == tickets.ElementAt(0).TicketId)), Times.Once());
            _mappingEngineMock.Verify(x => x.Map<ReleaseContent, ReleaseContentTicket>(It.Is<ReleaseContent>(y => y.TicketId == tickets.ElementAt(2).TicketId)), Times.Once());
            _mappingEngineMock.Verify(x => x.Map<ReleaseContent, ReleaseContentTicket>(It.IsAny<ReleaseContent>()), Times.Exactly(2));
        }

        [Test]
        public void GetTicketInformations_ShouldReturnNull_WhenReleaseWindowNotHavingAssociatedTickets()
        {
            var releaseWindow = new ReleaseWindow { ExternalId = Guid.NewGuid() };
            _releaseWindowRepositoryMock.SetupEntities(new[] { releaseWindow });

            var actual = Sut.GetTicketInformations(releaseWindow.ExternalId);

            Assert.IsNull(actual);
        }

        [Test]
        public void GetTicketInformations_ShouldReturnTickets_WhenReleaseWindowHaveAssociatedTickets()
        {
            var releaseWindow = new ReleaseWindow
            {
                ExternalId = Guid.NewGuid(),
                ReleaseContent = new List<ReleaseContent>
                {
                    new ReleaseContent { TicketId = Guid.NewGuid(), IncludeToReleaseNotes = true },
                    new ReleaseContent { TicketId = Guid.NewGuid(), IncludeToReleaseNotes = true },
                    new ReleaseContent { TicketId = Guid.NewGuid() }
                }
            };
            _releaseWindowRepositoryMock.SetupEntities(new[] { releaseWindow });
            _mappingEngineMock.Setup(x => x.Map<ReleaseContent, ReleaseContentTicket>(It.IsAny<ReleaseContent>()))
                .Returns(new ReleaseContentTicket());

            var actual = Sut.GetTicketInformations(releaseWindow.ExternalId);

            Assert.AreEqual(2, actual.Count());
            _mappingEngineMock.Verify(x => x.Map<ReleaseContent, ReleaseContentTicket>(It.Is<ReleaseContent>(
                y => y.TicketId == releaseWindow.ReleaseContent.ElementAt(0).TicketId)), Times.Once());
            _mappingEngineMock.Verify(x => x.Map<ReleaseContent, ReleaseContentTicket>(It.Is<ReleaseContent>(
                y => y.TicketId == releaseWindow.ReleaseContent.ElementAt(1).TicketId)), Times.Once());
        }

        [Test]
        public void AddOrUpdateTicketComment_ShouldInsertNewTicket_WhenEntityNotExists()
        {
            var accounts = BuildAccounts();

            var ticketInformations = BuildTicketInformations(accounts);

            var newValue = new ReleaseContentTicket
            {
                TicketId = Guid.NewGuid(),
                Comment = RandomData.RandomString(10),
                LastChangedByAccount = accounts.First().ExternalId
            };

            _accountRepositoryMock.SetupEntities(accounts);
            _releaseContentRepositoryMock.SetupEntities(ticketInformations);
            _releaseContentRepositoryMock.Setup(x => x.Insert(It.IsAny<ReleaseContent>()));
            _mappingEngineMock.Setup(x => x.Map<ReleaseContentTicket, ReleaseContent>(newValue))
                .Returns(new ReleaseContent { TicketId = newValue.TicketId, Comment = newValue.Comment });

            Sut.AddOrUpdateTicketComment(newValue);

            _releaseContentRepositoryMock.Verify(x => x.Insert(It.Is<ReleaseContent>(
                y => y.Comment == newValue.Comment && y.TicketId == newValue.TicketId && y.LastChangedByAccountId == accounts.First().AccountId)), Times.Once());
            _mappingEngineMock.Verify(x => x.Map<ReleaseContentTicket, ReleaseContent>(newValue), Times.Once());
        }

        [Test]
        public void AddOrUpdateTicketComment_ShouldUpdateTicketInformationWithNewComment_WhenEntityExists()
        {
            var accounts = BuildAccounts();

            var ticketInformations = BuildTicketInformations(accounts);

            var newValue = new ReleaseContentTicket
            {
                TicketId = new Guid(2, 0, 0, new byte[8]),
                Comment = RandomData.RandomString(10),
                LastChangedByAccount = accounts.First().ExternalId
            };

            _accountRepositoryMock.SetupEntities(accounts);
            _releaseContentRepositoryMock.SetupEntities(ticketInformations);
            _releaseContentRepositoryMock.Setup(x => x.Update(It.IsAny<ReleaseContent>()))
                .Returns(new ChangedFields<ReleaseContent>());

            Sut.AddOrUpdateTicketComment(newValue);

            _releaseContentRepositoryMock.Verify(x => x.Update(It.Is<ReleaseContent>(
                y => y.TicketId == newValue.TicketId && y.LastChangedByAccountId == accounts.First().AccountId)), Times.Once());
        }

        [Test]
        public void UpdateTicketReleaseRelation_ShouldUpdateTicketInformation()
        {
            var accounts = BuildAccounts();
            var account = new Account { ExternalId = accounts.First().ExternalId };

            var ticketInformations = BuildTicketInformations(accounts);

            var newValue = new ReleaseContentTicket
            {
                TicketId = new Guid(2, 0, 0, new byte[8]),
                IncludeToReleaseNotes = true,
                LastChangedByAccount = accounts.First().ExternalId
            };
            _releaseContentRepositoryMock.Setup(x => x.Update(It.IsAny<ReleaseContent>()))
                .Returns(new ChangedFields<ReleaseContent>());
            _accountRepositoryMock.SetupEntities(accounts);
            _releaseContentRepositoryMock.SetupEntities(ticketInformations);
            _mappingEngineMock.Setup(x => x.Map<ReleaseContentTicket, ReleaseContent>(newValue))
                .Returns(new ReleaseContent { TicketId = newValue.TicketId });

            Sut.UpdateTicketReleaseNotesRelation(newValue, account.ExternalId);

            _releaseContentRepositoryMock.Verify(x => x.Update(It.Is<ReleaseContent>(
                y =>
                    y.IncludeToReleaseNotes == newValue.IncludeToReleaseNotes && y.TicketId == newValue.TicketId &&
                    y.LastChangedByAccountId == accounts.First().AccountId)), Times.Once());
        }

        [Test]
        public void AddOrUpdateTicketRisk_ShouldInsertNewTicket_WhenEntityNotExists()
        {
            var accounts = BuildAccounts();

            var ticketInformations = BuildTicketInformations(accounts);

            var newValue = new ReleaseContentTicket
            {
                TicketId = Guid.NewGuid(),
                Risk = RandomData.RandomEnum<TicketRisk>(),
                LastChangedByAccount = accounts.First().ExternalId
            };

            _accountRepositoryMock.SetupEntities(accounts);
            _releaseContentRepositoryMock.SetupEntities(ticketInformations);
            _releaseContentRepositoryMock.Setup(x => x.Insert(It.IsAny<ReleaseContent>()));
            _mappingEngineMock.Setup(x => x.Map<ReleaseContentTicket, ReleaseContent>(newValue))
                .Returns(new ReleaseContent { TicketId = newValue.TicketId, TicketRisk = newValue.Risk });

            Sut.AddOrUpdateTicketRisk(newValue);

            _releaseContentRepositoryMock.Verify(x => x.Insert(It.Is<ReleaseContent>(
                y => y.TicketRisk == newValue.Risk && y.TicketId == newValue.TicketId && y.LastChangedByAccountId == accounts.First().AccountId)), Times.Once());
            _mappingEngineMock.Verify(x => x.Map<ReleaseContentTicket, ReleaseContent>(newValue), Times.Once());
        }

        [Test]
        public void CreateTicket_ShouldInsertNewTicket_WhenEntityNotExists()
        {
            var accounts = BuildAccounts();
            var account = new Account { ExternalId = accounts.First().ExternalId };

            var ticketInformations = BuildTicketInformations(accounts);

            var newValue = new ReleaseContentTicket
            {
                TicketId = Guid.NewGuid(),
                Risk = RandomData.RandomEnum<TicketRisk>(),
                IncludeToReleaseNotes = RandomData.RandomBool(),
                Comment = RandomData.RandomString(33)
            };

            _releaseContentRepositoryMock.SetupEntities(ticketInformations);
            _releaseContentRepositoryMock.Setup(x => x.Insert(It.IsAny<ReleaseContent>()));
            _mappingEngineMock.Setup(x => x.Map<ReleaseContentTicket, ReleaseContent>(newValue))
                .Returns(new ReleaseContent
                {
                    TicketId = newValue.TicketId,
                    TicketRisk = newValue.Risk,
                    Comment = newValue.Comment,
                    IncludeToReleaseNotes = newValue.IncludeToReleaseNotes
                });

            _accountRepositoryMock.SetupEntities(accounts);

            Sut.CreateTicket(newValue, account.ExternalId);

            _releaseContentRepositoryMock.Verify(x => x.Insert(It.Is<ReleaseContent>(
                y =>
                    y.TicketRisk == newValue.Risk && y.IncludeToReleaseNotes == newValue.IncludeToReleaseNotes &&
                    y.Comment == newValue.Comment && y.TicketId == newValue.TicketId &&
                    y.LastChangedByAccountId == accounts.First().AccountId)), Times.Once());
            _mappingEngineMock.Verify(x => x.Map<ReleaseContentTicket, ReleaseContent>(newValue), Times.Once());
        }

        [Test]
        public void AddOrUpdateTicketRisk_ShouldUpdateTicketInformationWithNewRisk_WhenEntityExists()
        {
            var accounts = BuildAccounts();

            var ticketInformations = BuildTicketInformations(accounts);

            var newValue = new ReleaseContentTicket
            {
                TicketId = new Guid(2, 0, 0, new byte[8]),
                Risk = RandomData.RandomEnum<TicketRisk>(),
                LastChangedByAccount = accounts.First().ExternalId
            };

            _accountRepositoryMock.SetupEntities(accounts);
            _releaseContentRepositoryMock.SetupEntities(ticketInformations);
            _releaseContentRepositoryMock.Setup(x => x.Update(It.IsAny<ReleaseContent>()))
                .Returns(new ChangedFields<ReleaseContent>());

            Sut.AddOrUpdateTicketRisk(newValue);

            _releaseContentRepositoryMock.Verify(x => x.Update(It.Is<ReleaseContent>(
                y => y.TicketRisk == newValue.Risk && y.TicketId == newValue.TicketId && y.LastChangedByAccountId == accounts.First().AccountId)), Times.Once());
        }

        [Test]
        public void AddOrUpdateTickets_ShouldAddOrUpdateTicketInformation_WhenInvoked()
        {
            var accounts = BuildAccounts();
            var account = new Account { ExternalId = accounts.First().ExternalId };

            var ticketInformations = BuildTicketInformations(accounts);

            var tickets = BuildTickets(accounts);

            var releaseWindow = new BusinessEntities.ReleaseCalendar.ReleaseWindow { ExternalId = Guid.NewGuid() };

            _accountRepositoryMock.SetupEntities(accounts);
            _releaseContentRepositoryMock.SetupEntities(ticketInformations);
            _releaseContentRepositoryMock.Setup(x => x.Update(It.IsAny<ReleaseContent>()))
                .Returns(new ChangedFields<ReleaseContent>());
            _releaseContentRepositoryMock.Setup(x => x.Insert(It.IsAny<ReleaseContent>()));
            _releaseWindowRepositoryMock.SetupEntities(new[] { new ReleaseWindow { ReleaseWindowId = 5, ExternalId = releaseWindow.ExternalId } });
            _mappingEngineMock.Setup(x => x.Map<ReleaseContentTicket, ReleaseContent>(tickets[2]))
                .Returns(new ReleaseContent
                {
                    TicketId = tickets[2].TicketId,
                    TicketRisk = tickets[2].Risk,
                    Comment = tickets[2].Comment,
                    IncludeToReleaseNotes = tickets[2].IncludeToReleaseNotes
                });

            Sut.AddOrUpdateTickets(tickets, account.ExternalId, releaseWindow.ExternalId);

            _releaseContentRepositoryMock.Verify(x => x.Update(It.Is<ReleaseContent>(
                y => y.TicketId == new Guid(2, 0, 0, new byte[8]) && y.LastChangedByAccountId == accounts.First().AccountId)), Times.Once());
            _releaseContentRepositoryMock.Verify(x => x.Update(It.Is<ReleaseContent>(
                y => y.TicketId == new Guid(3, 0, 0, new byte[8]) && y.LastChangedByAccountId == accounts.First().AccountId
                && !y.IncludeToReleaseNotes)), Times.Once());
            _releaseContentRepositoryMock.Verify(x => x.Insert(It.Is<ReleaseContent>(
                y =>
                    y.IncludeToReleaseNotes == tickets[2].IncludeToReleaseNotes &&
                    y.Comment == tickets[2].Comment && y.TicketId == tickets[2].TicketId &&
                    y.LastChangedByAccountId == accounts.First().AccountId)), Times.Once());
            _mappingEngineMock.Verify(x => x.Map<ReleaseContentTicket, ReleaseContent>(tickets[2]), Times.Once());
        }

        [Test]
        public void RemoveTicketsFromRelease_ShouldCleanReleaseWindowField_WhenInvoked()
        {
            var accounts = BuildAccounts();

            var releaseWindow = new BusinessEntities.ReleaseCalendar.ReleaseWindow { ExternalId = Guid.NewGuid() };

            var ticketInformations = BuildTicketInformations(accounts, releaseWindow);

            _releaseContentRepositoryMock.SetupEntities(ticketInformations);
            _releaseContentRepositoryMock.Setup(x => x.Update(It.IsAny<ReleaseContent>()))
               .Returns(new ChangedFields<ReleaseContent>());

            Sut.RemoveTicketsFromRelease(releaseWindow.ExternalId);

            _releaseContentRepositoryMock.Verify(x => x.Update(It.Is<ReleaseContent>(
                j => j.TicketId == new Guid(1, 0, 0, new byte[8]) && j.ReleaseWindow == null && j.ReleaseWindowsId == null)), Times.Once());

            _releaseContentRepositoryMock.Verify(x => x.Update(It.Is<ReleaseContent>(
                j => j.TicketId == new Guid(2, 0, 0, new byte[8]) && j.ReleaseWindow == null && j.ReleaseWindowsId == null)), Times.Once());

            _releaseContentRepositoryMock.Verify(x => x.Update(It.Is<ReleaseContent>(
                j => j.TicketId == new Guid(3, 0, 0, new byte[8]) && j.ReleaseWindow == null && j.ReleaseWindowsId == null)), Times.Once());

            _releaseContentRepositoryMock.Verify(x => x.Update(It.IsAny<ReleaseContent>()), Times.Exactly(3));
        }


        private static List<ReleaseContentTicket> BuildTickets(IList<DataEntities.Auth.Account> accounts)
        {
            return new List<ReleaseContentTicket>
            {
                new ReleaseContentTicket
                {
                    TicketId = new Guid(2, 0, 0, new byte[8]),
                    Risk = RandomData.RandomEnum<TicketRisk>(),
                    LastChangedByAccount = accounts.First().ExternalId
                },
                new ReleaseContentTicket
                {
                    TicketId = new Guid(3, 0, 0, new byte[8]),
                    Risk = RandomData.RandomEnum<TicketRisk>(),
                    LastChangedByAccount = accounts.First().ExternalId
                },
                new ReleaseContentTicket
                {
                    TicketId = new Guid(4, 0, 0, new byte[8]),
                    Risk = RandomData.RandomEnum<TicketRisk>(),
                    LastChangedByAccount = accounts.First().ExternalId
                }
            };
        }

        private static IEnumerable<ReleaseContent> BuildTicketInformations(IList<DataEntities.Auth.Account> accounts, BusinessEntities.ReleaseCalendar.ReleaseWindow releaseWindow = null)
        {
            return new List<ReleaseContent>
            {
                new ReleaseContent
                {
                    TicketId = new Guid(1, 0, 0, new byte[8]),
                    Comment = RandomData.RandomString(10),
                    LastChangedByAccount = accounts.First(),
                    ReleaseWindow = new ReleaseWindow{ExternalId = releaseWindow != null ? releaseWindow.ExternalId : Guid.NewGuid()},
                    ReleaseWindowsId = releaseWindow != null ? RandomData.RandomInt(1, 99) : (int?)null
                },
                new ReleaseContent
                {
                    TicketId = new Guid(2, 0, 0, new byte[8]),
                    Comment = RandomData.RandomString(10),
                    LastChangedByAccount = accounts.First(),
                    IncludeToReleaseNotes = false,
                    ReleaseWindow = new ReleaseWindow{ExternalId = releaseWindow != null ? releaseWindow.ExternalId : Guid.NewGuid()},
                    ReleaseWindowsId = releaseWindow != null ? RandomData.RandomInt(1, 99) : (int?)null
                },
                new ReleaseContent
                {
                    TicketId = new Guid(3, 0, 0, new byte[8]),
                    Comment = RandomData.RandomString(10),
                    LastChangedByAccount = accounts.First(),
                    ReleaseWindow = new ReleaseWindow{ExternalId = releaseWindow != null ? releaseWindow.ExternalId : Guid.NewGuid()},
                    ReleaseWindowsId = releaseWindow != null ? RandomData.RandomInt(1, 99) : (int?)null,
                    IncludeToReleaseNotes = true
                }
            };
        }

        private static IList<DataEntities.Auth.Account> BuildAccounts()
        {
            return Builder<DataEntities.Auth.Account>.CreateListOfSize(1).All()
                .Do(x => x.ExternalId = Guid.NewGuid())
                .Do(x => x.AccountId = RandomData.RandomInt(4))
                .Build();
        }
    }
}
