using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.ProductRequests;
using ReMi.BusinessLogic;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Plugins.Services.Email;
using ReMi.DataAccess.BusinessEntityGateways.ProductRequests;
using ReMi.DataAccess.Exceptions;
using ReMi.EventHandlers.ProductRequests;
using ReMi.Events.ProductRequests;

namespace ReMi.EventHandlers.Tests.ProductRequests
{
    public class ProductRequestRegistrationUpdatedEventHandlerTests : TestClassFor<ProductRequestRegistrationUpdatedEventHandler>
    {
        private Mock<IEmailService> _emailClientMock;
        private Mock<IEmailTextProvider> _emailTextProviderMock;
        private Mock<IProductRequestGateway> _productRequestGatewayMock;
        private Mock<IProductRequestRegistrationGateway> _productRequestRegistrationGatewayMock;

        protected override ProductRequestRegistrationUpdatedEventHandler ConstructSystemUnderTest()
        {
            return new ProductRequestRegistrationUpdatedEventHandler
            {
                EmailService = _emailClientMock.Object,
                EmailTextProvider = _emailTextProviderMock.Object,
                ProductRequestGatewayFactory = () => _productRequestGatewayMock.Object,
                ProductRequestRegistrationGatewayFactory = () => _productRequestRegistrationGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _emailClientMock = new Mock<IEmailService>();
            _emailTextProviderMock = new Mock<IEmailTextProvider>();
            _productRequestGatewayMock = new Mock<IProductRequestGateway>();
            _productRequestRegistrationGatewayMock = new Mock<IProductRequestRegistrationGateway>();

            base.TestInitialize();
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Handle_ShouldThrowException_WhenNoChangedTasks()
        {
            var evnt = PrepareEvent(Guid.NewGuid());

            Sut.Handle(evnt);
        }

        [Test]
        [ExpectedException(typeof(EntityNotFoundException))]
        public void Handle_ShouldThrowException_WhenRequestNotFound()
        {
            var taskId = Guid.NewGuid();

            var evnt = PrepareEvent(Guid.NewGuid(), taskId);

            Sut.Handle(evnt);
        }

        [Test]
        public void Handle_ShouldNotSendNotification_WhenNoAssignedAssignees()
        {
            var taskId = Guid.NewGuid();

            var evnt = PrepareEvent(Guid.NewGuid(), taskId);

            SetupRequestRegistrations(evnt.Registration, new[] { taskId });

            Sut.Handle(evnt);

            _emailClientMock.Verify(o => o.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        private ProductRequestRegistrationUpdatedEvent PrepareEvent(Guid externalId, Guid? changedTask = null)
        {
            var registration = Builder<ProductRequestRegistration>.CreateNew()
                .With(o => o.ExternalId, externalId)
                .Build();

            return new ProductRequestRegistrationUpdatedEvent
            {
                Context = Builder<EventContext>.CreateNew().Build(),
                Registration = registration,
                ChangedTasks = changedTask == null ? null : new[] { changedTask.Value }
            };
        }

        private void SetupRequestRegistrations(ProductRequestRegistration registration, IEnumerable<Guid> taskIds = null)
        {
            _productRequestRegistrationGatewayMock.Setup(o => o.GetRegistration(registration.ExternalId))
                .Returns(registration);

            if (taskIds != null)
            {
                var requestTasks = taskIds
                    .Select(o => Builder<ProductRequestTask>.CreateNew()
                                    .With(x => x.ExternalId, o)
                                    .Build())
                    .ToList();

                _productRequestGatewayMock.Setup(o => o.GetRequestGroupsByTasks(taskIds))
                    .Returns(new[]
                    {
                        Builder<ProductRequestGroup>.CreateNew()
                            .With(o => o.RequestTasks, requestTasks)
                            .Build()
                    });
            }
        }
    }
}
