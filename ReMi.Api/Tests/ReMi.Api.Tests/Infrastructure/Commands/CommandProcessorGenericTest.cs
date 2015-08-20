using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.Api.Insfrastructure.Commands;
using ReMi.Api.Insfrastructure.Security;
using ReMi.BusinessEntities.Auth;
using ReMi.Common.Constants.Auth;
using ReMi.Contracts.Cqrs;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.TestUtils.UnitTests;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;

namespace ReMi.Api.Tests.Infrastructure.Commands
{
    public class CommandProcessorGenericTest : TestClassFor<CommandProcessorGeneric<TestCommand>>
    {
        private Mock<IValidateRequest<TestCommand>> _validatorMock;
        private Mock<ICommandDispatcher> _commandDispatcherMock;
        private Mock<IAuthorizationManager> _authorizationManagerMock;
        private Mock<IPermissionChecker> _permissionCheckerMock;

        protected override CommandProcessorGeneric<TestCommand> ConstructSystemUnderTest()
        {
            _validatorMock = new Mock<IValidateRequest<TestCommand>>(MockBehavior.Strict);
            _commandDispatcherMock = new Mock<ICommandDispatcher>(MockBehavior.Strict);
            _authorizationManagerMock = new Mock<IAuthorizationManager>(MockBehavior.Strict);
            _permissionCheckerMock = new Mock<IPermissionChecker>(MockBehavior.Strict);

            return new CommandProcessorGeneric<TestCommand>
            {
                AuthorizationManager = _authorizationManagerMock.Object,
                CommandDispatcher = _commandDispatcherMock.Object,
                Validator = _validatorMock.Object,
                PermissionChecker = _permissionCheckerMock.Object
            };
        }

        [Test]
        public void HandleRequest_ShouldReturnStatusAccepted_WhenRequestSuccessfulyProcessed()
        {
            var commandId = Guid.NewGuid();
            var actionContext = BuildHttpActionContext(commandId);
            var command = BuildCommand(commandId);
            var account = BuildAccount();

            _authorizationManagerMock.Setup(x => x.Authenticate(actionContext))
                .Returns(account);
            _authorizationManagerMock.Setup(x => x.IsAuthorized(It.IsAny<IEnumerable<Role>>()))
                .Returns(true);
            _permissionCheckerMock.Setup(x => x.CheckCommandPermission(typeof(TestCommand), account))
                .Returns(PermissionStatus.Permmited);
            _permissionCheckerMock.Setup(x => x.CheckRule(account, command))
                .Returns(PermissionStatus.Permmited);
            _validatorMock.Setup(x => x.ValidateRequest(command)).Returns((IEnumerable<ValidationError>)null);
            _commandDispatcherMock.Setup(x => x.Send(command))
                .Returns((Task)null);

            var response = Sut.HandleRequest(actionContext, command);

            _authorizationManagerMock.Verify(x => x.Authenticate(actionContext));
            _validatorMock.Verify(x => x.ValidateRequest(command));
            _commandDispatcherMock.Verify(x => x.Send(command));

            Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
            Assert.AreEqual(account.ExternalId, command.CommandContext.UserId);
            Assert.AreEqual(account.FullName, command.CommandContext.UserName);
            Assert.IsNull(response.Content);
        }

        [Test]
        public void HandleRequest_ShouldReturnStatusForbidden_WhenNoSession()
        {
            var commandId = Guid.NewGuid();
            var actionContext = BuildHttpActionContext(commandId);
            var command = BuildCommand(commandId);

            _authorizationManagerMock.Setup(x => x.Authenticate(actionContext))
                .Returns((Account)null);
            _permissionCheckerMock.Setup(x => x.CheckCommandPermission(typeof(TestCommand), null))
                .Returns(PermissionStatus.NotAuthenticated);

            var response = Sut.HandleRequest(actionContext, command);

            _authorizationManagerMock.Verify(x => x.IsAuthorized(It.IsAny<Role[]>()), Times.Never());

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Test]
        public void HandleRequest_ShouldReturnStatusUnauthorized_WhenUserDoNotHavePersmissions()
        {
            var commandId = Guid.NewGuid();
            var actionContext = BuildHttpActionContext(commandId);
            var command = BuildCommand(commandId);
            var account = BuildAccount();

            _authorizationManagerMock.Setup(x => x.Authenticate(actionContext))
                .Returns(account);
            _permissionCheckerMock.Setup(x => x.CheckCommandPermission(typeof(TestCommand), account))
                .Returns(PermissionStatus.NotAuthorized);

            var response = Sut.HandleRequest(actionContext, command);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public void HandleRequest_ShouldReturnStatusUnauthorized_WhenRuleIsAppliedAndFail()
        {
            var commandId = Guid.NewGuid();
            var actionContext = BuildHttpActionContext(commandId);
            var command = BuildCommand(commandId);
            var account = BuildAccount();

            _authorizationManagerMock.Setup(x => x.Authenticate(actionContext))
                .Returns(account);
            _permissionCheckerMock.Setup(x => x.CheckCommandPermission(typeof(TestCommand), account))
                .Returns(PermissionStatus.Permmited);
            _permissionCheckerMock.Setup(x => x.CheckRule(account, command))
                .Returns(PermissionStatus.NotAuthorized);

            var response = Sut.HandleRequest(actionContext, command);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public void HandleRequest_ShouldReturnStatusNotAcceptable_WhenCommandIsInvalid()
        {
            var commandId = Guid.NewGuid();
            var actionContext = BuildHttpActionContext(commandId);
            var command = BuildCommand(commandId);
            var account = BuildAccount();
            var errorMessage = BuildErrorMessageCollection();

            _authorizationManagerMock.Setup(x => x.Authenticate(actionContext))
                .Returns(account);
            _authorizationManagerMock.Setup(x => x.IsAuthorized(It.IsAny<IEnumerable<Role>>()))
                .Returns(true);
            _validatorMock.Setup(x => x.ValidateRequest(command))
                .Returns(errorMessage);
            _permissionCheckerMock.Setup(x => x.CheckCommandPermission(typeof(TestCommand), account))
                .Returns(PermissionStatus.Permmited);
            _permissionCheckerMock.Setup(x => x.CheckRule(account, command))
                .Returns(PermissionStatus.Permmited);

            var response = Sut.HandleRequest(actionContext, command);

            _validatorMock.Verify(x => x.ValidateRequest(command));
            _commandDispatcherMock.Verify(x => x.Send(command), Times.Never());

            Assert.AreEqual(HttpStatusCode.NotAcceptable, response.StatusCode);
            Assert.AreEqual(((ObjectContent)response.Content).Value, errorMessage);
        }

        private static IEnumerable<ValidationError> BuildErrorMessageCollection()
        {
            return Builder<ValidationError>.CreateListOfSize(1)
                .All()
                .WithConstructor(() => new ValidationError(RandomData.RandomString(5), RandomData.RandomString(5)))
                .Build();
        }

        private static TestCommand BuildCommand(Guid commandId)
        {
            return Builder<TestCommand>.CreateNew()
                .With(x => x.CommandContext, Builder<CommandContext>.CreateNew()
                    .With(y => y.Id, commandId)
                    .Build())
                .Build();
        }

        private static HttpActionContext BuildHttpActionContext(Guid commandId)
        {
            var request = new HttpRequestMessage();
            request.Headers.Add("CommandId", commandId.ToString());
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

    public class TestCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }
    }
}
