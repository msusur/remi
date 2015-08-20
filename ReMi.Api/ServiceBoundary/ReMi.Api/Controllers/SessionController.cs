using System;
using ReMi.Api.Insfrastructure.Security;
using ReMi.Common.WebApi;
using ReMi.Queries.Auth;
using System.Web;
using System.Web.Http;
using ReMi.BusinessEntities.Exceptions;

namespace ReMi.Api.Controllers
{
    [RoutePrefix("session")]
    public class SessionController : ApiController
    {
        public IImplementQueryAction<GetActiveSessionRequest, GetActiveSessionResponse> GetActiveSessionAction { get; set; }

        public IImplementQueryAction<GetNewSessionRequest, GetActiveSessionResponse> GetNewSessionAction { get; set; }
        
        [HttpGet]
        [Route("check")]
        public IHttpActionResult CheckActiveSession()
        {
            var currentTokenParts = HttpTokenHelper.GetTokenParts(HttpContext.Current.Request);

            if (currentTokenParts == null) return Unauthorized();

            var request = new GetActiveSessionRequest
            {
                SessionId = currentTokenParts.SessionId,
                UserName = currentTokenParts.UserName
            };

            var response = GetActiveSessionAction.Handle(ActionContext, request);
            if (response != null && response.Account != null && response.Session != null)
            {
                response.Token = HttpTokenHelper.GenerateToken(response.Account.Name, response.Session.ExternalId);

                return Ok(response);
            }

            return Unauthorized();
        }

        [HttpGet]
        [Route("{sessionId}")]
        public GetActiveSessionResponse StartSession(Guid sessionId)
        {
            var request = new GetNewSessionRequest
            {
                SessionId = sessionId
            };

            var response = GetNewSessionAction.Handle(ActionContext, request);
            if (response != null && response.Account != null && response.Session != null)
            {
                response.Token = HttpTokenHelper.GenerateToken(response.Account.Name, response.Session.ExternalId);
                return response;
            }

            throw new FailedToAuthenticateException(response != null && response.Account != null ? response.Account.FullName : sessionId.ToString());
        }
    }
}
