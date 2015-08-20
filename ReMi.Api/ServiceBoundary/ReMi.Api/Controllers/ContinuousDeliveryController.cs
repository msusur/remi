using System;
using System.Linq;
using System.Net;
using System.Web.Http.Results;
using ReMi.Api.Insfrastructure.Security;
using ReMi.BusinessLogic.Api;
using ReMi.Commands.ContinuousDelivery;
using ReMi.Common.Constants.Auth;
using ReMi.Common.Cqrs;
using ReMi.Common.WebApi;
using ReMi.Queries.ContinuousDelivery;
using System.Web.Http;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Api.Controllers
{
    [RoutePrefix("cd")]
    public class ContinuousDeliveryController : ApiController
    {
        public IImplementQueryAction<GetContinuousDeliveryStatusRequest, GetContinuousDeliveryStatusResponse> GetContinuousDeliveryStatus { get; set; }
        public IHandleCommand<UpdateApiCommand> UpdateApiHandler { get; set; }
        public IPermissionChecker PermissionChecker { get; set; }
        public IAuthorizationManager AuthorizationManager { get; set; }
        public IApiDescriptionBuilder ApiBuilder { get; set; }

        public IImplementQueryAction<GetApiDescriptionRequest, GetApiDescriptionResponse> GetApiDescriptionAction { get; set; }

        [HttpGet]
        [Route("status/{product}")]
        public GetContinuousDeliveryStatusResponse GetCheckList(string product)
        {
            var request = new GetContinuousDeliveryStatusRequest
            {
                Products = new[] { product }
            };

            return GetContinuousDeliveryStatus.Handle(ActionContext, request);
        }

        [HttpPost]
        [Route("updateapi")]
        public IHttpActionResult UpdateApi()
        {
            try
            {
                var account = AuthorizationManager.Authenticate(ActionContext);
                switch (PermissionChecker.CheckCommandPermission(typeof(UpdateApiCommand), account))
                {
                    case PermissionStatus.NotAuthenticated:
                        return new StatusCodeResult(HttpStatusCode.Forbidden, ActionContext.Request);
                    case PermissionStatus.NotAuthorized: return Unauthorized();
                }

                var descriptions = ApiBuilder.GetApiDescriptions().ToList();

                var command = new UpdateApiCommand
                {
                    ApiDescriptions = descriptions
                };

                UpdateApiHandler.Handle(command);
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("apiDescriptions")]
        public GetApiDescriptionResponse GetApiDescription()
        {
            var request = new GetApiDescriptionRequest();

            return GetApiDescriptionAction.Handle(ActionContext, request);
        }
    }
}
