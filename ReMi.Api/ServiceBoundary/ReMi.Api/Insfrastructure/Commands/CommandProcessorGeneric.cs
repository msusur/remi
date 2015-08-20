using Common.Logging;
using ReMi.Api.Insfrastructure.Security;
using ReMi.Common.Constants.Auth;
using ReMi.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using ReMi.Contracts.Cqrs;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Api.Insfrastructure.Commands
{
    public class CommandProcessorGeneric<TCommand> : ICommandProcessorGeneric<TCommand> where TCommand : class, ICommand
    {
        public IValidateRequest<TCommand> Validator { get; set; }
        public ICommandDispatcher CommandDispatcher { get; set; }
        public IPermissionChecker PermissionChecker { get; set; }
        public IAuthorizationManager AuthorizationManager { get; set; }

        private readonly ILog _logger = LogManager.GetLogger(typeof(CommandProcessorGeneric<>));

        public HttpResponseMessage HandleRequest(HttpActionContext actionContext, TCommand command)
        {
            if (command == null)
                throw new ArgumentNullException("command", "Command cannot be null");
            if (actionContext == null)
                throw new ArgumentNullException("actionContext", "ActionContext cannot be null");

            var httpStatus = CheckPermissions(command, actionContext);
            if (httpStatus == HttpStatusCode.Forbidden || httpStatus == HttpStatusCode.Unauthorized)
            {
                return actionContext.Request.CreateResponse(httpStatus);
            }

            var errors = Validate(command);
            if (!errors.IsNullOrEmpty())
            {
                _logger.ErrorFormat("Invalid request: {0}", string.Join("; ", errors.Select(e => string.Format("{0}: {1}", e.PropertyName, e.ErrorMessage))));
                return actionContext.Request.CreateResponse(HttpStatusCode.NotAcceptable, errors);
            }

            CommandDispatcher.Send(command);

            return actionContext.Request.CreateResponse(httpStatus);
        }

        private HttpStatusCode CheckPermissions(TCommand command, HttpActionContext actionContext)
        {
            var account = AuthorizationManager.Authenticate(actionContext);
            if (account != null)
            {
                command.CommandContext.UserEmail = account.Email;
                command.CommandContext.UserName = account.FullName;
                command.CommandContext.UserId = account.ExternalId;
                command.CommandContext.UserRole = account.Role.Name;
            }

            var permissionStatus = PermissionChecker.CheckCommandPermission(typeof(TCommand), account);

            if (permissionStatus == PermissionStatus.Permmited)
            {
                permissionStatus = PermissionChecker.CheckRule(account, command);
            }
            switch (permissionStatus)
            {
                case PermissionStatus.NotAuthenticated: return HttpStatusCode.Forbidden;
                case PermissionStatus.NotAuthorized: return HttpStatusCode.Unauthorized;
                case PermissionStatus.Permmited: return HttpStatusCode.Accepted;
                default: return HttpStatusCode.Forbidden;
            }
        }

        private IEnumerable<ValidationError> Validate(TCommand command)
        {
            return Validator == null ? Enumerable.Empty<ValidationError>() : Validator.ValidateRequest(command);
        }
    }
}
