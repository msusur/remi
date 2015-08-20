using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Api;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessLogic.Api;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.Api;
using ReMi.DataAccess.BusinessEntityGateways.Auth;
using ReMi.Queries.ContinuousDelivery;
using ReMi.QueryHandlers.ContinuousDelivery;

namespace ReMi.QueryHandlers.Tests.ContinuousDelivery
{
    public class GetApiDescriptionHandlerTests : TestClassFor<GetApiDescriptionHandler>
    {
        private Mock<IApiDescriptionGateway> _apiDescriptionGatewayMock;
        private Mock<ICommandPermissionsGateway> _commandPermissionsGatewayMock;
        private Mock<IQueryPermissionsGateway> _queryPermissionsGatewayMock;
        private Mock<IApiDescriptionBuilder> _apiDescriptionBuilderMock;
        private Mock<IMappingEngine> _mappingEngineMock;

        protected override GetApiDescriptionHandler ConstructSystemUnderTest()
        {
            return new GetApiDescriptionHandler
            {
                ApiBuilder = _apiDescriptionBuilderMock.Object,
                ApiDescriptionGatewayFactory = () => _apiDescriptionGatewayMock.Object,
                CommandPermissionsGatewayFactory = () => _commandPermissionsGatewayMock.Object,
                QueryPermissionsGatewayFactory = () => _queryPermissionsGatewayMock.Object,
                MappingEngine = _mappingEngineMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _mappingEngineMock = new Mock<IMappingEngine>();
            _apiDescriptionGatewayMock = new Mock<IApiDescriptionGateway>();
            _commandPermissionsGatewayMock = new Mock<ICommandPermissionsGateway>();
            _queryPermissionsGatewayMock = new Mock<IQueryPermissionsGateway>();

            _mappingEngineMock = new Mock<IMappingEngine>();
            _mappingEngineMock.Setup(o => o.Map<ApiDescription, ApiDescriptionFull>(It.IsAny<ApiDescription>()))
                .Returns<ApiDescription>(o => new ApiDescriptionFull
                {
                    Description = o.Description,
                    InputFormat = o.InputFormat,
                    Method = o.Method,
                    Name = o.Name,
                    OutputFormat = o.OutputFormat,
                    Url = o.Url
                });

            _apiDescriptionBuilderMock = new Mock<IApiDescriptionBuilder>();
            _apiDescriptionBuilderMock.Setup(o => o.GetApiDescriptions())
                .Returns(ConstructApiDescriptions());

            SetupApiDescriptionGateway();

            SetupCommandPermissionsGateway();

            SetupQueryPermissionsGateway();

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldPopulateDescriptionFromDB_WhenInvoked()
        {
            var request = CreateRequest();

            var result = Sut.Handle(request);

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.ApiDescriptions.Count());
            Assert.AreEqual("description1", result.ApiDescriptions.First().Description);
            Assert.AreEqual("description2", result.ApiDescriptions.ElementAt(1).Description);
            Assert.IsNull(result.ApiDescriptions.ElementAt(2).Description);
        }

        [Test]
        public void Handle_ShouldPopulateCommandInformationFromDb_WhenInvoked()
        {
            var request = CreateRequest();

            var result = Sut.Handle(request);

            Assert.AreEqual("R 1, R 2", result.ApiDescriptions.First().Roles);
            Assert.AreEqual("command desc", result.ApiDescriptions.First().DescriptionShort);
            Assert.AreEqual("command group", result.ApiDescriptions.First().Group);
        }

        [Test]
        public void Handle_ShouldPopulateQueryInformationFromDb_WhenInvoked()
        {
            var request = CreateRequest();

            var result = Sut.Handle(request);

            Assert.AreEqual("R 3, R 4", result.ApiDescriptions.ElementAt(1).Roles);
            Assert.AreEqual("query desc", result.ApiDescriptions.ElementAt(1).DescriptionShort);
            Assert.AreEqual("query group", result.ApiDescriptions.ElementAt(1).Group);
        }

        [Test]
        public void Handle_ShouldNotPopulateCommandInformationFromDb_WhenAccordingRowsNotFound()
        {
            var request = CreateRequest();

            var result = Sut.Handle(request);

            Assert.IsNull(result.ApiDescriptions.ElementAt(2).Roles);
            Assert.IsNull(result.ApiDescriptions.ElementAt(2).DescriptionShort);
            Assert.IsNull(result.ApiDescriptions.ElementAt(2).Group);
        }

        private GetApiDescriptionRequest CreateRequest()
        {
            return new GetApiDescriptionRequest
            {
                Context = new QueryContext()
            };
        }

        private IEnumerable<ApiDescription> ConstructApiDescriptions()
        {
            var result = new List<ApiDescription>
            {
                Builder<ApiDescription>.CreateNew()
                    .With(o => o.Url, "http://ref1")
                    .With(o => o.Description, null)
                    .With(o => o.Method, "Post")
                    .With(o => o.Name, "ComRef1")
                    .Build(),
                Builder<ApiDescription>.CreateNew()
                    .With(o => o.Url, "http://ref2")
                    .With(o => o.Name, "QueryRef2")
                    .With(o => o.Description, null)
                    .With(o => o.Method, "Get")
                    .Build(),
                Builder<ApiDescription>.CreateNew()
                    .With(o => o.Url, "http://ref3")
                    .With(o => o.Name, "ComRef3")
                    .With(o => o.Description, null)
                    .With(o => o.Method, "Post")
                    .Build()
            };

            return result;
        }

        private void SetupApiDescriptionGateway()
        {
            var result = new List<ApiDescription>
            {
                Builder<ApiDescription>.CreateNew()
                    .With(o => o.Url, "http://ref1")
                    .With(o => o.Description, "description1")
                    .Build(),
                Builder<ApiDescription>.CreateNew()
                    .With(o => o.Url, "http://ref2")
                    .With(o => o.Description, "description2")
                    .Build(),
                Builder<ApiDescription>.CreateNew()
                    .With(o => o.Url, "http://ref4")
                    .With(o => o.Description, "description4")
                    .Build()
            };

            _apiDescriptionGatewayMock.Setup(o => o.GetApiDescriptions())
                .Returns(result);
        }

        private void SetupCommandPermissionsGateway()
        {
            var result = new List<Command>
            {
                Builder<Command>.CreateNew()
                    .With(o => o.Name, "ComRef1")
                    .With(o => o.Group, "command group")
                    .With(o => o.Description, "command desc")
                    .With(o => o.Roles, new[]
                        {
                            new Role{ Description = "R 1", Name = "R1"},
                            new Role{ Description = "R 2", Name = "R2"}
                        })
                    .Build()
            };

            _commandPermissionsGatewayMock.Setup(o => o.GetCommands(false))
                .Returns(result);
        }

        private void SetupQueryPermissionsGateway()
        {
            var result = new List<Query>
            {
                Builder<Query>.CreateNew()
                    .With(o => o.Name, "QueryRef2")
                    .With(o => o.Group, "query group")
                    .With(o => o.Description, "query desc")
                    .With(o => o.Roles, new[]
                        {
                            new Role{ Description = "R 3", Name = "R3"},
                            new Role{ Description = "R 4", Name = "R4"}
                        })
                    .Build()
            };

            _queryPermissionsGatewayMock.Setup(o => o.GetQueries(false))
                .Returns(result);
        }
    }
}
