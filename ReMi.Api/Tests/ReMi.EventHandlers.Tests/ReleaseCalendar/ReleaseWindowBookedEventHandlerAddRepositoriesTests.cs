using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.DeploymentTool;
using ReMi.BusinessEntities.Products;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessLogic.ReleasePlan;
using ReMi.Commands.SourceControl;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Plugins.Services.DeploymentTool;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.EventHandlers.ReleaseCalendar;
using ReMi.Events.ReleaseCalendar;

namespace ReMi.EventHandlers.Tests.ReleaseCalendar
{
    public class ReleaseWindowBookedEventHandlerAddRepositoriesTests : TestClassFor<ReleaseWindowBookedEventHandlerAddRepositories>
    {
        private Mock<ICommandDispatcher> _commandDispatcherMock;
        private Mock<IReleaseWindowHelper> _releaseWindowHelperMock;

        protected override ReleaseWindowBookedEventHandlerAddRepositories ConstructSystemUnderTest()
        {
            return new ReleaseWindowBookedEventHandlerAddRepositories
            {
                ReleaseWindowHelper = _releaseWindowHelperMock.Object,
                CommandDispatcher = _commandDispatcherMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseWindowHelperMock = new Mock<IReleaseWindowHelper>(MockBehavior.Strict);
            _commandDispatcherMock = new Mock<ICommandDispatcher>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldDoNothing_WhenReleaseIsMaintenance()
        {
            var evnt = new ReleaseWindowBookedEvent
            {
                ReleaseWindow = new ReleaseWindow { ReleaseType = ReleaseType.SystemMaintenance }
            };

            _releaseWindowHelperMock.Setup(o => o.IsMaintenance(evnt.ReleaseWindow))
                .Returns(true);

            Sut.Handle(evnt);

            _releaseWindowHelperMock.Verify(o => o.IsMaintenance(It.IsAny<ReleaseWindow>()), Times.Once);
            _commandDispatcherMock.Verify(x => x.Send(It.IsAny<ICommand>()), Times.Never);
        }

        [Test]
        public void Handle_ShouldSendCommandToLoadRepositories_WhenReleaseIsNotMaintenance()
        {
            var evnt = new ReleaseWindowBookedEvent
            {
                ReleaseWindow = new ReleaseWindow { ReleaseType = ReleaseType.Scheduled, ExternalId = Guid.NewGuid()},
                Context = new EventContext { Id = Guid.NewGuid() }
            };

            _releaseWindowHelperMock.Setup(o => o.IsMaintenance(evnt.ReleaseWindow))
                .Returns(false);
            _commandDispatcherMock.Setup(x => x.Send(It.Is<LoadReleaseRepositoriesCommand>(
                c => c.ReleaseWindowId == evnt.ReleaseWindow.ExternalId && c.CommandContext.ParentId == evnt.Context.Id)))
                .Returns((Task) null);

            Sut.Handle(evnt);

            _releaseWindowHelperMock.Verify(o => o.IsMaintenance(It.IsAny<ReleaseWindow>()), Times.Once);
            _commandDispatcherMock.Verify(x => x.Send(It.IsAny<ICommand>()), Times.Once);
        }
    }
}
