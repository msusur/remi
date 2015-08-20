using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Api;
using ReMi.CommandHandlers.ContinuousDelivery;
using ReMi.Commands.ContinuousDelivery;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Events;
using ReMi.DataAccess.BusinessEntityGateways.Api;
using ReMi.Events.Api;

namespace ReMi.CommandHandlers.Tests.ContinuousDelivery
{
    public class UpdateApiCommandHandlerTests : TestClassFor<UpdateApiCommandHandler>
    {
        private Mock<IApiDescriptionGateway> _apiDescriptionGatewayMock;
        private Mock<IPublishEvent> _eventPublisherMock; 
        private List<ApiDescription> _descriptions;

        protected override UpdateApiCommandHandler ConstructSystemUnderTest()
        {
            return new UpdateApiCommandHandler
            {
                ApiDescriptionGatewayFactory = () => _apiDescriptionGatewayMock.Object,
                EventPublisher = _eventPublisherMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _apiDescriptionGatewayMock = new Mock<IApiDescriptionGateway>();
            _eventPublisherMock = new Mock<IPublishEvent>();
            _descriptions = new List<ApiDescription>
            {
                new ApiDescription
                {
                    Method = RandomData.RandomString(3, 5),
                    Url = RandomData.RandomString(5, 17)
                },
                new ApiDescription
                {
                    Method = RandomData.RandomString(6, 7),
                    Url = RandomData.RandomString(18)
                }
            };

            _apiDescriptionGatewayMock.Setup(x => x.GetApiDescriptions()).Returns(_descriptions);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldManageDescriptionsCorrectly()
        {
            var command = new UpdateApiCommand
            {
                ApiDescriptions = new List<ApiDescription>
                {
                    _descriptions[0],
                    new ApiDescription
                    {
                        Method = RandomData.RandomString(6, 7),
                        Url = RandomData.RandomString(11)
                    }
                }
            };

            Sut.Handle(command);

            _apiDescriptionGatewayMock.Verify(x => x.GetApiDescriptions());

            _apiDescriptionGatewayMock.Verify(
                x => x.RemoveApiDescriptions(It.Is<List<ApiDescription>>(l => l.Count == 1 && l[0]
                                                                              == _descriptions[1])));

            _apiDescriptionGatewayMock.Verify(
                x => x.CreateApiDescriptions(It.Is<List<ApiDescription>>(l => l.Count == 1 && l[0]
                                                                              == command.ApiDescriptions[1])));

            _eventPublisherMock.Verify(e => e.Publish(It.Is<ApiUpdatedEvent>(x => x.AddedDescriptions.Count == 1 &&
                                                                                  x.RemovedDescriptions.Count == 1)));
        }

        [Test]
        public void Handle_ShouldOnlyAddNewDescription()
        {
            var command = new UpdateApiCommand
            {
                ApiDescriptions = new List<ApiDescription>
                {
                    _descriptions[0],
                    _descriptions[1],
                    new ApiDescription
                    {
                        Method = RandomData.RandomString(6, 7),
                        Url = RandomData.RandomString(11)
                    }
                }
            };

            Sut.Handle(command);

            _apiDescriptionGatewayMock.Verify(x => x.GetApiDescriptions());

            _apiDescriptionGatewayMock.Verify(
                x => x.RemoveApiDescriptions(It.IsAny<List<ApiDescription>>()), Times.Never);

            _apiDescriptionGatewayMock.Verify(
                x => x.CreateApiDescriptions(It.Is<List<ApiDescription>>(l => l.Count == 1 && l[0]
                                                                              == command.ApiDescriptions[2])));
            _eventPublisherMock.Verify(e => e.Publish(It.Is<ApiUpdatedEvent>(x => x.AddedDescriptions.Count == 1 &&
                                                                                  x.RemovedDescriptions == null)));
        }

        [Test]
        public void Handle_ShouldOnlyRemoveObsoleteDescription()
        {
            var command = new UpdateApiCommand
            {
                ApiDescriptions = new List<ApiDescription>
                {
                    _descriptions[0],
                }
            };

            Sut.Handle(command);

            _apiDescriptionGatewayMock.Verify(x => x.GetApiDescriptions());

            _apiDescriptionGatewayMock.Verify(
                x => x.CreateApiDescriptions(It.IsAny<List<ApiDescription>>()), Times.Never);

            _apiDescriptionGatewayMock.Verify(
                x => x.RemoveApiDescriptions(It.Is<List<ApiDescription>>(l => l.Count == 1 && l[0]
                                                                              == _descriptions[1])));
            _eventPublisherMock.Verify(e => e.Publish(It.Is<ApiUpdatedEvent>(x => x.AddedDescriptions == null &&
                                                                                  x.RemovedDescriptions.Count == 1)));
        }
    }
}
