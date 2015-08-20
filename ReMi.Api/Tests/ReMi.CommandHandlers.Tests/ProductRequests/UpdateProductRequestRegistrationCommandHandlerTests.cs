using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ProductRequests;
using ReMi.CommandHandlers.ProductRequests;
using ReMi.Commands.ProductRequests;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.ProductRequests;
using ReMi.Events.ProductRequests;

namespace ReMi.CommandHandlers.Tests.ProductRequests
{
    public class UpdateProductRequestRegistrationCommandHandlerTests : TestClassFor<UpdateProductRequestRegistrationCommandHandler>
    {
        private Mock<IProductRequestRegistrationGateway> _productRequestRegistrationGatewayMock;
        private Mock<IPublishEvent> _publishEventMock;

        protected override UpdateProductRequestRegistrationCommandHandler ConstructSystemUnderTest()
        {
            return new UpdateProductRequestRegistrationCommandHandler
            {
                ProductRequestRegistrationGatewayFactory = () => _productRequestRegistrationGatewayMock.Object,
                PublishEvent = _publishEventMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _productRequestRegistrationGatewayMock = new Mock<IProductRequestRegistrationGateway>();
            _publishEventMock = new Mock<IPublishEvent>();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldCallGateway_WhenInvoked()
        {
            var registrationId = Guid.NewGuid();

            var registration = SetupGateway(registrationId);

            var command = BuildCommand(registration);

            Sut.Handle(command);

            _productRequestRegistrationGatewayMock.Verify(o => o.UpdateProductRequestRegistration(registration), Times.Once);
        }

        [Test]
        public void Handle_ShouldSetChangedByAccountId_WhenInvoked()
        {
            var registrationId = Guid.NewGuid();

            var registration = SetupGateway(registrationId);

            var command = BuildCommand(registration);

            Sut.Handle(command);

            _productRequestRegistrationGatewayMock.Verify(o =>
                o.UpdateProductRequestRegistration(
                    It.Is<ProductRequestRegistration>(x => x.CreatedByAccountId == command.CommandContext.UserId)), Times.Once);
        }

        [Test]
        public void Handle_ShouldNotSendNotification_WhenTasksEmpty()
        {
            var registrationId = Guid.NewGuid();

            var registration = SetupGateway(registrationId);

            var command = BuildCommand(registration);

            Sut.Handle(command);

            _publishEventMock.Verify(o => o.Publish(It.IsAny<IEvent>()), Times.Never);
        }

        [Test]
        public void Handle_ShouldSendNotification_WhenTasksAdded()
        {
            var registrationId = Guid.NewGuid();

            var taskId = Guid.NewGuid();

            var existingTasks = new ProductRequestRegistrationTask[0];
            var updatedTasks = new[] { new ProductRequestRegistrationTask { ProductRequestTaskId = taskId } };

            var registration = SetupGateway(registrationId, updatedTasks, existingTasks);

            var command = BuildCommand(registration);

            Sut.Handle(command);

            _publishEventMock.Verify(o => o.Publish(It.Is<ProductRequestRegistrationUpdatedEvent>(ev =>
                ev.ChangedTasks.Count() == 1 && ev.ChangedTasks.First() == taskId)), Times.Exactly(1));
        }

        [Test]
        public void Handle_ShouldNotSendNotification_WhenExistingTasksNotChanged()
        {
            var registrationId = Guid.NewGuid();

            var taskId = Guid.NewGuid();

            var existingTasks = new[] { new ProductRequestRegistrationTask { ProductRequestTaskId = taskId, IsCompleted = false } };
            var updatedTasks = new[] { new ProductRequestRegistrationTask { ProductRequestTaskId = taskId, IsCompleted = false } };

            var registration = SetupGateway(registrationId, updatedTasks, existingTasks);

            var command = BuildCommand(registration);

            Sut.Handle(command);

            _publishEventMock.Verify(o => o.Publish(It.IsAny<IEvent>()), Times.Never);
        }

        [Test]
        public void Handle_ShouldSendNotification_WhenIsCompletedChanged()
        {
            var registrationId = Guid.NewGuid();

            var taskId = Guid.NewGuid();

            var existingTasks = new[] { new ProductRequestRegistrationTask { ProductRequestTaskId = taskId, IsCompleted = false } };
            var updatedTasks = new[] { new ProductRequestRegistrationTask { ProductRequestTaskId = taskId, IsCompleted = true } };

            var registration = SetupGateway(registrationId, updatedTasks, existingTasks);

            var command = BuildCommand(registration);

            Sut.Handle(command);

            _publishEventMock.Verify(o => o.Publish(It.Is<ProductRequestRegistrationUpdatedEvent>(ev =>
                ev.ChangedTasks.Count() == 1 && ev.ChangedTasks.First() == taskId)), Times.Exactly(1));
        }

        [Test]
        public void Handle_ShouldSendNotification_WhenCommentChanged()
        {
            var registrationId = Guid.NewGuid();

            var taskId = Guid.NewGuid();

            var existingTasks = new[] { new ProductRequestRegistrationTask { ProductRequestTaskId = taskId, Comment = null } };
            var updatedTasks = new[] { new ProductRequestRegistrationTask { ProductRequestTaskId = taskId, Comment = "AAA" } };

            var registration = SetupGateway(registrationId, updatedTasks, existingTasks);

            var command = BuildCommand(registration);

            Sut.Handle(command);

            _publishEventMock.Verify(o => o.Publish(It.Is<ProductRequestRegistrationUpdatedEvent>(ev =>
                ev.ChangedTasks.Count() == 1 && ev.ChangedTasks.First() == taskId)), Times.Exactly(1));
        }

        [Test]
        public void Handle_ShouldSendOnlyChangedNotification_WhenInvoked()
        {
            var registrationId = Guid.NewGuid();

            var taskId = Guid.NewGuid();

            var existingTasks = new[]
            {
                new ProductRequestRegistrationTask { ProductRequestTaskId = taskId, Comment = null }, 
                new ProductRequestRegistrationTask { ProductRequestTaskId = Guid.NewGuid(), Comment = null }
            };
            var updatedTasks = new[]
            {
                new ProductRequestRegistrationTask { ProductRequestTaskId = taskId, Comment = "AAA" },
                new ProductRequestRegistrationTask { ProductRequestTaskId = Guid.NewGuid(), Comment = "Not existing task" }
            };

            var registration = SetupGateway(registrationId, updatedTasks, existingTasks);

            var command = BuildCommand(registration);

            Sut.Handle(command);

            _publishEventMock.Verify(o => o.Publish(It.Is<ProductRequestRegistrationUpdatedEvent>(ev =>
                ev.ChangedTasks.Count() == 1 && ev.ChangedTasks.First() == taskId)), Times.Exactly(1));
        }

        private ProductRequestRegistration SetupGateway(Guid externalId,
            IEnumerable<ProductRequestRegistrationTask> updatedTasks = null,
            IEnumerable<ProductRequestRegistrationTask> existingTasks = null)
        {
            var registration = Builder<ProductRequestRegistration>.CreateNew()
                .With(o => o.ExternalId, externalId)
                .With(o => o.CreatedByAccountId, Guid.Empty)
                .With(o => o.Tasks, updatedTasks)
                .Build();

            var existingRegistration = Builder<ProductRequestRegistration>.CreateNew()
               .With(o => o.ExternalId, externalId)
               .With(o => o.CreatedByAccountId, Guid.Empty)
               .With(o => o.Tasks, existingTasks)
               .Build();

            _productRequestRegistrationGatewayMock.Setup(o => o.GetRegistration(externalId))
                .Returns(existingRegistration);

            return registration;
        }

        private UpdateProductRequestRegistrationCommand BuildCommand(ProductRequestRegistration registration)
        {
            var command = new UpdateProductRequestRegistrationCommand
            {
                CommandContext = new CommandContext
                {
                    UserId = Guid.NewGuid()
                },
                Registration = registration
            };

            return command;
        }
    }
}
