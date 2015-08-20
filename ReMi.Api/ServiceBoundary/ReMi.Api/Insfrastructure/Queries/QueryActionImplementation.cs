using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Controllers;
using ReMi.Api.Insfrastructure.Security;
using ReMi.BusinessEntities.Auth;
using ReMi.Common.Constants.Auth;
using ReMi.Common.Cqrs;
using ReMi.Common.Utils;
using ReMi.Common.WebApi;
using ReMi.Common.WebApi.Exceptions;
using ReMi.Contracts.Cqrs;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Api.Insfrastructure.Queries
{
    public class QueryActionImplementation<TRequest, TResponse> : ActionImplementationBase<TRequest>, IImplementQueryAction<TRequest, TResponse>
        where TRequest : IQuery
    {
        public ISerialization Serialization { get; set; }
        public IHandleQuery<TRequest, TResponse> Handler { get; set; }
        public IAuthorizationManager AuthorizationManager { get; set; }
        public IPermissionChecker PermissionChecker { get; set; }
        public IClientRequestInfoRetriever ClientRequestInfoRetriever { get; set; }
        public IApplicationSettings ApplicationSettings { get; set; }

        public TResponse Handle(HttpActionContext actionContext, TRequest request)
        {
            var response = default(TResponse);
            var errors = Enumerable.Empty<ValidationError>();

            try
            {
                AssertRequestNotNull(request);

                var account = GetRequestAccount(actionContext);

                var httpStatus = CheckPermissions(account, request);

                switch (httpStatus)
                {
                    case HttpStatusCode.Forbidden:
                        ThrowHttpStatusError(httpStatus, "User is not authenticated");
                        break;
                    case HttpStatusCode.Unauthorized:
                        ThrowHttpStatusError(httpStatus, "Access Denied");
                        break;
                }

                errors = ResolveAndInvokeValidateRequest(request);

                if (errors.IsNullOrEmpty())
                {
                    Logger.DebugFormat("{0} successfully validated by {1}.", typeof(TRequest).Name, GetType().Name);

                    FillContext(request, account);

                    response = ResolveAndInvokeHandle(request);
                }
            }
            catch (HttpResponseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ThrowHttpInternalServerErrorAndLog(ex);
            }

            if (!errors.IsNullOrEmpty())
            {
                ThrowHttpNotAcceptableAndLog(errors);
            }

            return response;

        }

        private Account GetRequestAccount(HttpActionContext actionContext)
        {
            return AuthorizationManager.Authenticate(actionContext);
        }

        private HttpStatusCode CheckPermissions(Account account, TRequest request)
        {
            var permissionStatus = PermissionChecker.CheckQueryPermission(typeof(TRequest), account);
            if (permissionStatus == PermissionStatus.Permmited)
            {
                permissionStatus = PermissionChecker.CheckRule(account, request);
            }
            switch (permissionStatus)
            {
                case PermissionStatus.NotAuthenticated: return HttpStatusCode.Forbidden;
                case PermissionStatus.NotAuthorized: return HttpStatusCode.Unauthorized;
                case PermissionStatus.Permmited: return HttpStatusCode.OK;
                default: return HttpStatusCode.Forbidden;
            }
        }

        private void FillContext(TRequest request, Account account)
        {
            if (request.Context == null)
                request.Context = new QueryContext
                {
                    UserEmail = account != null ? account.Email : string.Empty,
                    UserName = account != null ? account.FullName : string.Empty,
                    UserId = account != null ? account.ExternalId : Guid.Empty,
                    UserRole = account != null ? account.Role.Name : string.Empty,
                    UserHostAddress = ClientRequestInfoRetriever.UserHostAddress,
                    UserHostName = ClientRequestInfoRetriever.UserHostName
                };
        }

        private TResponse ResolveAndInvokeHandle(TRequest request)
        {
            if (Handler == null)
                throw new QueryHandlerNotImplementedException<TRequest, TResponse>();

            Logger.InfoFormat("Query '{0}' was handled: {1}", typeof(TRequest).Name, FormatLogHelper.FormatEntry(Serialization, ApplicationSettings, request));

            var response = Handler.Handle(request);

            if (ApplicationSettings.LogQueryResponses)
                Logger.InfoFormat("Response for query '{0}' has been received: {1}", typeof(TRequest).Name, FormatLogHelper.FormatEntry(Serialization, ApplicationSettings, response));

            return response;
        }
    }
}
