using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Products;
using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Common.Constants.ReleasePlan;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Services.ReleaseContent;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Queries.ReleasePlan;
using ReMi.QueryHandlers.ReleasePlan;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReMi.QueryHandlers.Tests.ReleasePlan
{
    public class GetReleaseContentInformationHandlerTests : TestClassFor<GetReleaseContentInformationHandler>
    {
        private Mock<IProductGateway> _productGatewayMock;
        private Mock<IReleaseContentGateway> _releaseContentGatewayMock;
        private Mock<IReleaseContent> _releaseContentMock;
        private Mock<IMappingEngine> _mappingEngineMock;

        protected override GetReleaseContentInformationHandler ConstructSystemUnderTest()
        {
            return new GetReleaseContentInformationHandler
            {
                ProductGatewayFactory = () => _productGatewayMock.Object,
                ReleaseContent = _releaseContentMock.Object,
                ReleaseContentGateway = () => _releaseContentGatewayMock.Object,
                MappingEngine = _mappingEngineMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _mappingEngineMock = new Mock<IMappingEngine>();
            _productGatewayMock = new Mock<IProductGateway>();
            _releaseContentGatewayMock = new Mock<IReleaseContentGateway>();
            _releaseContentMock = new Mock<IReleaseContent>();
            _productGatewayMock.Setup(x => x.Dispose());
            _releaseContentGatewayMock.Setup(x => x.Dispose());

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldGetTicketsFromGateway_WhenInvoked()
        {
            var releaseWindowId = Guid.NewGuid();
            var product = Builder<Product>.CreateNew()
                .With(r => r.Description, RandomData.RandomString(5))
                .With(r => r.ExternalId, Guid.NewGuid())
                .Build();

            var issues = Builder<Contracts.Plugins.Data.ReleaseContent.ReleaseContentTicket>.CreateListOfSize(5)
                .All()
                .Do(x => x.TicketName = RandomData.RandomString(6))
                .Do(x => x.TicketDescription = RandomData.RandomString(100))
                .Do(x => x.Assignee = RandomData.RandomString(10))
                .Build();


            _productGatewayMock.Setup(g => g.GetProducts(releaseWindowId))
                .Returns(new[] { product });

            _releaseContentGatewayMock.Setup(x => x.GetTicketInformations(releaseWindowId))
                .Returns((IEnumerable<ReleaseContentTicket>)null);
            _releaseContentGatewayMock.Setup(x => x.GetTicketInformations(It.IsAny<IEnumerable<Guid>>()))
                .Returns((IEnumerable<ReleaseContentTicket>)null);


            _releaseContentMock.Setup(x => x.GetTickets(It.IsAny<IEnumerable<Guid>>()))
                .Returns(issues);
            _mappingEngineMock.Setup(x => x.Map<IEnumerable<Contracts.Plugins.Data.ReleaseContent.ReleaseContentTicket>, IEnumerable<ReleaseContentTicket>>(issues))
                .Returns(issues.Select(x => new ReleaseContentTicket { TicketId = x.TicketId, TicketUrl = "url" }));

            var result = Sut.Handle(new GetReleaseContentInformationRequest { ReleaseWindowId = releaseWindowId });

            _productGatewayMock.Verify(r => r.GetProducts(releaseWindowId));
            _releaseContentMock.Verify(cd => cd.GetTickets(It.Is<IEnumerable<Guid>>(c => c.Count() == 1)));

            Assert.AreEqual(5, result.Content.Count());
            Assert.IsTrue(result.Content.All(x => x.TicketUrl.StartsWith("url")));
        }

        [Test]
        public void Handle_ShouldGetTicketsWithComentAndRisk_WhenInvokedAndTicketInformationExists()
        {
            var releaseWindowId = Guid.NewGuid();
            var product = Builder<Product>.CreateNew()
                .With(r => r.Description, RandomData.RandomString(5))
                .With(r => r.ExternalId, Guid.NewGuid())
                .Build();

            var issues = Builder<Contracts.Plugins.Data.ReleaseContent.ReleaseContentTicket>.CreateListOfSize(5)
                .All()
                .Do(x => x.TicketName = RandomData.RandomString(6))
                .Do(x => x.TicketDescription = RandomData.RandomString(100))
                .Do(x => x.Assignee = RandomData.RandomString(10))
                .Build();

            var ticketInformations = Builder<ReleaseContentTicket>.CreateListOfSize(2)
                .All()
                .Do(x => x.Comment = RandomData.RandomString(10))
                .Do(x => x.Risk = RandomData.RandomEnum<TicketRisk>())
                .Do(x => x.IncludeToReleaseNotes = RandomData.RandomBool())
                .TheFirst(1)
                .Do(x => x.TicketId = issues[1].TicketId)
                .TheNext(1)
                .Do(x => x.TicketId = issues[3].TicketId)
                .Build();


            _productGatewayMock.Setup(g => g.GetProducts(releaseWindowId))
                .Returns(new[] { product });

            _releaseContentMock.Setup(x => x.GetTickets(It.IsAny<IEnumerable<Guid>>()))
                .Returns(issues);
            _mappingEngineMock.Setup(x => x.Map<IEnumerable<Contracts.Plugins.Data.ReleaseContent.ReleaseContentTicket>, IEnumerable<ReleaseContentTicket>>(issues))
                .Returns(issues.Select(x => new ReleaseContentTicket { TicketId = x.TicketId, TicketUrl = "url" }));
            _releaseContentGatewayMock.Setup(x => x.GetTicketInformations(It.IsAny<IEnumerable<Guid>>()))
                .Returns(ticketInformations);
            _releaseContentGatewayMock.Setup(x => x.GetTicketInformations(releaseWindowId))
                .Returns((IEnumerable<ReleaseContentTicket>)null);


            var result = Sut.Handle(new GetReleaseContentInformationRequest { ReleaseWindowId = releaseWindowId });

            _productGatewayMock.Verify(r => r.GetProducts(releaseWindowId));
            _releaseContentMock.Verify(cd => cd.GetTickets(It.Is<IEnumerable<Guid>>(c => c.Count() == 1)));
            Assert.AreEqual(5, result.Content.Count(), "ticket list size");
            Assert.AreEqual(ticketInformations[0].Comment, result.Content.ElementAt(1).Comment, "comment");
            Assert.AreEqual(ticketInformations[0].Risk, result.Content.ElementAt(1).Risk, "risk");
            Assert.AreEqual(ticketInformations[0].IncludeToReleaseNotes, result.Content.ElementAt(1).IncludeToReleaseNotes,
                "include to release notes");
            Assert.AreEqual(ticketInformations[1].Comment, result.Content.ElementAt(3).Comment, "comment");
            Assert.AreEqual(ticketInformations[1].Risk, result.Content.ElementAt(3).Risk, "risk");
            Assert.AreEqual(ticketInformations[1].IncludeToReleaseNotes, result.Content.ElementAt(3).IncludeToReleaseNotes,
                "include to release notes");
        }

        [Test]
        public void Handle_ShouldGetTicketsFromDb_WhenTicketsExistsInDb()
        {
            var releaseWindowId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            _releaseContentGatewayMock.Setup(x => x.GetTicketInformations(releaseWindowId))
                .Returns(new[] { new ReleaseContentTicket { TicketId = ticketId } });

            var result = Sut.Handle(new GetReleaseContentInformationRequest { ReleaseWindowId = releaseWindowId });

            Assert.AreEqual(1, result.Content.Count());
            Assert.AreEqual(ticketId, result.Content.First().TicketId);

            _releaseContentGatewayMock.Verify(x => x.GetTicketInformations(releaseWindowId), Times.Once());
        }
    }
}
