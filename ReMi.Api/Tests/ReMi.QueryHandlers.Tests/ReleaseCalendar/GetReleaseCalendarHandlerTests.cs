using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Commands.ReleaseCalendar;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.Queries.ReleaseCalendar;
using ReMi.QueryHandlers.ReleaseCalendar;

namespace ReMi.QueryHandlers.Tests.ReleaseCalendar
{
    public class GetReleaseCalendarHandlerTests : TestClassFor<GetReleaseCalendarHandler>
    {
        private Mock<IReleaseWindowGateway> _releaseWindowGatewayMock;
        private Mock<IMappingEngine> _mappingEngineMock;
        private Mock<ICommandDispatcher> _commandDispatcherMock;

        protected override GetReleaseCalendarHandler ConstructSystemUnderTest()
        {
            return new GetReleaseCalendarHandler
            {
                ReleaseWindowGatewayFactory = () => _releaseWindowGatewayMock.Object,
                MappingEngine = _mappingEngineMock.Object,
                CommandDispatcher = _commandDispatcherMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _mappingEngineMock = new Mock<IMappingEngine>();
            _mappingEngineMock.Setup(
                o => o.Map<GetReleaseCalendarRequest, ReleaseCalendarFilter>(It.IsAny<GetReleaseCalendarRequest>()))
                .Returns<GetReleaseCalendarRequest>(request => new ReleaseCalendarFilter { StartDay = request.StartDay, EndDay = request.EndDay });

            _releaseWindowGatewayMock = new Mock<IReleaseWindowGateway>();

            _commandDispatcherMock = new Mock<ICommandDispatcher>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldGetReleaseCalendar_WhenInvoked()
        {
            var request = Builder<GetReleaseCalendarRequest>.CreateNew().Build();

            Sut.Handle(request);

            _releaseWindowGatewayMock.Verify(o => o.GetAllStartingInTimeRange(
                request.StartDay.ToUniversalTime(),
                request.EndDay.AddDays(1).AddMilliseconds(-1).ToUniversalTime()));
        }

        [Test]
        public void Handle_ShouldGetTwoReleaseWindows_WhenThereAreTwoBookedReleases()
        {
            const int releaseCount = 2;

            var request = Builder<GetReleaseCalendarRequest>.CreateNew().Build();
            var releaseList = Builder<ReleaseWindowView>
                                    .CreateListOfSize(releaseCount)
                                    .Build();

            SetupMappingEngineWithGetReleaseCalendarResponse(releaseList);

            _releaseWindowGatewayMock.Setup(o => o.GetAllStartingInTimeRange(
                request.StartDay.ToUniversalTime(),
                request.EndDay.AddDays(1).AddMilliseconds(-1).ToUniversalTime()))
                .Returns(releaseList);

            var response = Sut.Handle(request);

            Assert.AreEqual(releaseCount, response.ReleaseWindows.Count());
        }

        [Test]
        public void Handle_ShouldNotSendCloseExpiredReleasesCommand_WhenInvokedByNonAuthenticatedUser()
        {
            var request = Builder<GetReleaseCalendarRequest>.CreateNew().Build();

            Sut.Handle(request);

            _commandDispatcherMock.Verify(me => me.Send(It.IsAny<CloseExpiredReleasesCommand>()), Times.Never);
        }

        [Test]
        public void Handle_ShouldMapRequestToFilter_WhenInvoked()
        {
            var request = Builder<GetReleaseCalendarRequest>.CreateNew().Build();

            Sut.Handle(request);

            _mappingEngineMock.Verify(me => me.Map<GetReleaseCalendarRequest, ReleaseCalendarFilter>(request));
        }

        private void SetupMappingEngineWithGetReleaseCalendarResponse(IEnumerable<ReleaseWindowView> releases)
        {
            _mappingEngineMock.Setup(me => me.Map<IEnumerable<ReleaseWindowView>, GetReleaseCalendarResponse>(releases))
                .Returns(new GetReleaseCalendarResponse
                {
                    ReleaseWindows = new List<ReleaseWindowView>(releases.Select(r => new ReleaseWindowView()))
                });
        }

    }
}
