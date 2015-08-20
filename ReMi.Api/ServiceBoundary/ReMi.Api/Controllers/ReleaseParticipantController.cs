using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using ReMi.Api.Insfrastructure.Filters;
using ReMi.BusinessEntities.Auth;
using ReMi.Common.WebApi;
using ReMi.Queries.ReleaseParticipant;

namespace ReMi.Api.Controllers
{
    [RoutePrefix("releaseParticipants")]
    public class ReleaseParticipantController : ApiController
    {
        public IImplementQueryAction<GetReleaseParticipantRequest, GetReleaseParticipantResponse>
            GetReleaseParticipantAction { get; set; }

        [HttpGet]
        [Route("{releaseWindowId:Guid}")]
        public GetReleaseParticipantResponse GetReleaseParticipants(Guid releaseWindowId)
        {
            var request = new GetReleaseParticipantRequest {ReleaseWindowId = releaseWindowId};

            return GetReleaseParticipantAction.Handle(ActionContext, request);
        }
    }
}
