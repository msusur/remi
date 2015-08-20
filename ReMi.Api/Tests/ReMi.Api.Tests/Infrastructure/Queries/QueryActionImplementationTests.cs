using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.Api.Insfrastructure;
using ReMi.Api.Insfrastructure.Queries;
using ReMi.Api.Insfrastructure.Security;
using ReMi.BusinessEntities.Auth;
using ReMi.Common.Constants.Auth;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.TestUtils.UnitTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;

namespace ReMi.Api.Tests.Infrastructure.Queries
{
    [TestFixture]
    public class QueryActionImplementationTests : TestClassFor<QueryActionImplementation<TestRequest, TestResponse>>
    {
        private Mock<IHandleQuery<TestRequest, TestResponse>> _handlerMock;
        private Mock<IValidateRequest<TestRequest>> _validatorMock;
        private Mock<IAuthorizationManager> _authorizationManagerMock;
        private Mock<IPermissionChecker> _permissionCheckerMock;
        private Mock<ISerialization> _serializationMock;
        private Mock<IClientRequestInfoRetriever> _clientRequestInfoRetrieverMock;
        private Mock<IApplicationSettings> _applicationSettingsMock;

        protected override QueryActionImplementation<TestRequest, TestResponse> ConstructSystemUnderTest()
        {
            _handlerMock = new Mock<IHandleQuery<TestRequest, TestResponse>>(MockBehavior.Strict);
            _validatorMock = new Mock<IValidateRequest<TestRequest>>(MockBehavior.Strict);
            _authorizationManagerMock = new Mock<IAuthorizationManager>(MockBehavior.Strict);
            _permissionCheckerMock = new Mock<IPermissionChecker>(MockBehavior.Strict);
            _serializationMock = new Mock<ISerialization>(MockBehavior.Strict);
            _clientRequestInfoRetrieverMock = new Mock<IClientRequestInfoRetriever>(MockBehavior.Strict);
            _applicationSettingsMock = new Mock<IApplicationSettings>(MockBehavior.Strict);
            _applicationSettingsMock.SetupGet(x => x.LogJsonFormatted).Returns(true);
            _applicationSettingsMock.SetupGet(x => x.LogQueryResponses).Returns(false);

            return new QueryActionImplementation<TestRequest, TestResponse>
            {
                AuthorizationManager = _authorizationManagerMock.Object,
                Validator = _validatorMock.Object,
                PermissionChecker = _permissionCheckerMock.Object,
                Handler = _handlerMock.Object,
                Serialization = _serializationMock.Object,
                ClientRequestInfoRetriever = _clientRequestInfoRetrieverMock.Object,
                ApplicationSettings = _applicationSettingsMock.Object,
            };
        }

        [Test]
        public void Handle_ShouldReturnExpectedResponse_WhenRequestSuccessfulyProcessed()
        {
            var actionContext = BuildHttpActionContext();
            var query = BuildQuery();
            var account = BuildAccount();
            var expectedResponse = new TestResponse();

            _authorizationManagerMock.Setup(x => x.Authenticate(actionContext))
                .Returns(account);
            _authorizationManagerMock.Setup(x => x.IsAuthorized(It.IsAny<IEnumerable<Role>>()))
                .Returns(true);
            _permissionCheckerMock.Setup(x => x.CheckQueryPermission(typeof(TestRequest), account))
                .Returns(PermissionStatus.Permmited);
            _permissionCheckerMock.Setup(x => x.CheckRule(account, query))
                .Returns(PermissionStatus.Permmited);
            _validatorMock.Setup(x => x.ValidateRequest(query)).Returns((IEnumerable<ValidationError>)null);
            _handlerMock.Setup(x => x.Handle(query))
                .Returns(expectedResponse);
            _serializationMock.Setup(x => x.ToJson(query, It.Is<IEnumerable<string>>(s => s.First() == "Password"), It.IsAny<bool>()))
                .Returns("json data");
            _clientRequestInfoRetrieverMock.SetupGet(x => x.UserHostAddress).Returns("host address");
            _clientRequestInfoRetrieverMock.SetupGet(x => x.UserHostName).Returns("host name");

            var response = Sut.Handle(actionContext, query);

            _authorizationManagerMock.Verify(x => x.Authenticate(actionContext));
            _validatorMock.Verify(x => x.ValidateRequest(query));
            _handlerMock.Verify(x => x.Handle(query));
            _serializationMock.Verify(x => x.ToJson(It.IsAny<object>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>()), Times.Once);

            Assert.AreEqual(expectedResponse, response);
        }

        [Test]
        public void Handle_ShouldWriteQueryResponse_WhenFeatureTurnedOn()
        {
            var actionContext = BuildHttpActionContext();
            var query = BuildQuery();
            var account = BuildAccount();
            var expectedResponse = new TestResponse();
            
            _applicationSettingsMock.SetupGet(x => x.LogQueryResponses).Returns(true);

            _authorizationManagerMock.Setup(x => x.Authenticate(actionContext))
                .Returns(account);
            _authorizationManagerMock.Setup(x => x.IsAuthorized(It.IsAny<IEnumerable<Role>>()))
                .Returns(true);
            _permissionCheckerMock.Setup(x => x.CheckQueryPermission(typeof(TestRequest), account))
                .Returns(PermissionStatus.Permmited);
            _permissionCheckerMock.Setup(x => x.CheckRule(account, query))
                .Returns(PermissionStatus.Permmited);
            _validatorMock.Setup(x => x.ValidateRequest(query)).Returns((IEnumerable<ValidationError>)null);
            _handlerMock.Setup(x => x.Handle(query))
                .Returns(expectedResponse);
            _serializationMock.Setup(x => x.ToJson(query, It.Is<IEnumerable<string>>(s => s.First() == "Password"), It.IsAny<bool>()))
                .Returns("json data");
            _serializationMock.Setup(x => x.ToJson(It.IsAny<TestResponse>(), It.Is<IEnumerable<string>>(s => s.First() == "Password"), It.IsAny<bool>()))
                .Returns("response");
            _clientRequestInfoRetrieverMock.SetupGet(x => x.UserHostAddress).Returns("host address");
            _clientRequestInfoRetrieverMock.SetupGet(x => x.UserHostName).Returns("host name");

            var response = Sut.Handle(actionContext, query);

            _serializationMock.Verify(x => x.ToJson(It.IsAny<object>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>()), Times.Exactly(2));
        }

        [Test]
        public void Handle_ShouldReturnStatusForbidden_WhenNoSession()
        {
            var actionContext = BuildHttpActionContext();
            var query = BuildQuery();

            _authorizationManagerMock.Setup(x => x.Authenticate(actionContext))
                .Returns((Account)null);
            _permissionCheckerMock.Setup(x => x.CheckQueryPermission(typeof(TestRequest), null))
                .Returns(PermissionStatus.NotAuthenticated);

            HttpResponseException result = null;
            try
            {
                Sut.Handle(actionContext, query);
            }
            catch (HttpResponseException ex)
            {
                result = ex;
            }

            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.Forbidden, result.Response.StatusCode);
        }

        [Test]
        public void Handle_ShouldReturnStatusUnauthorized_WhenUserDoNotHavePersmissions()
        {
            var actionContext = BuildHttpActionContext();
            var query = BuildQuery();
            var account = BuildAccount();

            _authorizationManagerMock.Setup(x => x.Authenticate(actionContext))
                .Returns(account);
            _permissionCheckerMock.Setup(x => x.CheckQueryPermission(typeof(TestRequest), account))
                .Returns(PermissionStatus.NotAuthorized);

            HttpResponseException result = null;
            try
            {
                Sut.Handle(actionContext, query);
            }
            catch (HttpResponseException ex)
            {
                result = ex;
            }

            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.Response.StatusCode);
        }
        [Test]
        public void Handle_ShouldReturnStatusUnauthorized_WhenRuleIsAppliedAndFail()
        {
            var actionContext = BuildHttpActionContext();
            var query = BuildQuery();
            var account = BuildAccount();

            _authorizationManagerMock.Setup(x => x.Authenticate(actionContext))
                .Returns(account);
            _permissionCheckerMock.Setup(x => x.CheckQueryPermission(typeof(TestRequest), account))
                .Returns(PermissionStatus.Permmited);
            _permissionCheckerMock.Setup(x => x.CheckRule(account, query))
                .Returns(PermissionStatus.NotAuthorized);

            HttpResponseException result = null;
            try
            {
                Sut.Handle(actionContext, query);
            }
            catch (HttpResponseException ex)
            {
                result = ex;
            }

            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.Response.StatusCode);
        }

        [Test]
        public void Handle_ShouldReturnStatusNotAcceptable_WhenCommandIsInvalid()
        {
            var actionContext = BuildHttpActionContext();
            var query = BuildQuery();
            var account = BuildAccount();
            var errorMessage = BuildErrorMessageCollection();

            _authorizationManagerMock.Setup(x => x.Authenticate(actionContext))
                .Returns(account);
            _authorizationManagerMock.Setup(x => x.IsAuthorized(It.IsAny<IEnumerable<Role>>()))
                .Returns(true);
            _validatorMock.Setup(x => x.ValidateRequest(query))
                .Returns(errorMessage);
            _permissionCheckerMock.Setup(x => x.CheckQueryPermission(typeof(TestRequest), account))
                .Returns(PermissionStatus.Permmited);
            _permissionCheckerMock.Setup(x => x.CheckRule(account, query))
                .Returns(PermissionStatus.Permmited);

            HttpResponseException result = null;
            try
            {
                Sut.Handle(actionContext, query);
            }
            catch (HttpResponseException ex)
            {
                result = ex;
            }

            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.NotAcceptable, result.Response.StatusCode);
        }

        [Test]
        public void Handle_ShouldPopulateContext_WhenProcessed()
        {
            var actionContext = BuildHttpActionContext();
            var query = BuildQuery();
            var account = BuildAccount();
            var expectedResponse = new TestResponse();

            _authorizationManagerMock.Setup(x => x.Authenticate(actionContext))
                .Returns(account);
            _authorizationManagerMock.Setup(x => x.IsAuthorized(It.IsAny<IEnumerable<Role>>()))
                .Returns(true);
            _permissionCheckerMock.Setup(x => x.CheckQueryPermission(typeof(TestRequest), account))
                .Returns(PermissionStatus.Permmited);
            _permissionCheckerMock.Setup(x => x.CheckRule(account, query))
                .Returns(PermissionStatus.Permmited);
            _validatorMock.Setup(x => x.ValidateRequest(query)).Returns((IEnumerable<ValidationError>)null);
            _handlerMock.Setup(x => x.Handle(query))
                .Returns(expectedResponse);
            _serializationMock.Setup(x => x.ToJson(query, It.Is<IEnumerable<string>>(s => s.First() == "Password"), It.IsAny<bool>()))
                .Returns("json data");
            _clientRequestInfoRetrieverMock.SetupGet(x => x.UserHostAddress).Returns("host address");
            _clientRequestInfoRetrieverMock.SetupGet(x => x.UserHostName).Returns("host name");

            Sut.Handle(actionContext, query);

            Assert.AreNotEqual(query.Context.UserId, Guid.Empty);

            Assert.AreEqual(account.ExternalId, query.Context.UserId);
        }

        [Test]
        public void Handle_ShouldNotPopulateContext_WhenAnonymousQueryProcessed()
        {
            var actionContext = BuildHttpActionContext();
            var query = BuildQuery();
            var account = (Account) null;
            var expectedResponse = new TestResponse();

            _authorizationManagerMock.Setup(x => x.Authenticate(actionContext)).Returns(account);
            _authorizationManagerMock.Setup(x => x.IsAuthorized(It.IsAny<IEnumerable<Role>>()))
                .Returns(true);
            _permissionCheckerMock.Setup(x => x.CheckQueryPermission(typeof(TestRequest), account))
                .Returns(PermissionStatus.Permmited);
            _permissionCheckerMock.Setup(x => x.CheckRule(account, query))
                .Returns(PermissionStatus.Permmited);
            _validatorMock.Setup(x => x.ValidateRequest(query)).Returns((IEnumerable<ValidationError>)null);
            _handlerMock.Setup(x => x.Handle(query))
                .Returns(expectedResponse);
            _serializationMock.Setup(x => x.ToJson(query, It.Is<IEnumerable<string>>(s => s.First() == "Password"), It.IsAny<bool>()))
                .Returns("json data");
            _clientRequestInfoRetrieverMock.SetupGet(x => x.UserHostAddress).Returns("host address");
            _clientRequestInfoRetrieverMock.SetupGet(x => x.UserHostName).Returns("host name");

            Sut.Handle(actionContext, query);

            Assert.AreEqual(query.Context.UserId, Guid.Empty);
            Assert.AreEqual("host address", query.Context.UserHostAddress);
            Assert.AreEqual("host name", query.Context.UserHostName);
        }

        private static IEnumerable<ValidationError> BuildErrorMessageCollection()
        {
            return Builder<ValidationError>.CreateListOfSize(1)
                .All()
                .WithConstructor(() => new ValidationError(RandomData.RandomString(5), RandomData.RandomString(5)))
                .Build();
        }

        private static TestRequest BuildQuery()
        {
            return Builder<TestRequest>.CreateNew()
                .Build();
        }

        private static HttpActionContext BuildHttpActionContext()
        {
            var request = new HttpRequestMessage();
            request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            var actionContext = new HttpActionContext
            {
                ControllerContext = new HttpControllerContext
                {
                    Request = request
                }
            };

            return actionContext;
        }

        private static Account BuildAccount()
        {
            return Builder<Account>.CreateNew()
                .With(x => x.FullName, RandomData.RandomString(5))
                .With(x => x.ExternalId, Guid.NewGuid())
                .With(x => x.Role, new Role { Name = "Admin" })
                .Build();
        }
    }

    public class TestRequest : IQuery
    {
        public QueryContext Context { get; set; }
    }

    public class TestResponse
    {

    }
}
