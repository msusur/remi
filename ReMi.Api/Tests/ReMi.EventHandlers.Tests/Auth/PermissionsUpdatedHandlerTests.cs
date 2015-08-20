using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.EventHandlers.Auth;
using ReMi.Events.Auth;
using ReMi.Queries.Auth;

namespace ReMi.EventHandlers.Tests.Auth
{
    public class PermissionsUpdatedHandlerTests : TestClassFor<PermissionsUpdatedEventHandler>
    {
        private Mock<IHandleQuery<PermissionsRequest, PermissionsResponse>> _permissionsQueryMock;
        private Mock<IPublishEvent> _eventPublisherMock; 

        protected override PermissionsUpdatedEventHandler ConstructSystemUnderTest()
        {
            return new PermissionsUpdatedEventHandler
            {
                EventPublisher = _eventPublisherMock.Object,
                PermissionsQuery = _permissionsQueryMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _eventPublisherMock = new Mock<IPublishEvent>();
            _permissionsQueryMock = new Mock<IHandleQuery<PermissionsRequest, PermissionsResponse>>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldSentEventToUI()
        {
            var evnt = new PermissionsUpdatedEvent
            {
                RoleId = Guid.NewGuid()
            };
            var response = new PermissionsResponse
            {
                Commands = new List<String>(),
                Queries = new List<String>()
            };
            _permissionsQueryMock.Setup(p => p.Handle(It.Is<PermissionsRequest>(s => s.RoleId == evnt.RoleId)))
                .Returns(response);

            Sut.Handle(evnt);

            _permissionsQueryMock.Verify(p => p.Handle(It.Is<PermissionsRequest>(s => s.RoleId == evnt.RoleId)));
            _eventPublisherMock.Verify(
                e =>
                    e.Publish(
                        It.Is<PermissionsUpdatedUiEvent>(
                            p =>
                                p.RoleId == evnt.RoleId && p.Commands == response.Commands &&
                                p.Queries == response.Queries)));
        }
    }
}
