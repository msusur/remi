using System.Web.Http;
using ReMi.Common.WebApi;
using ReMi.Queries.ReleasePlan;

namespace ReMi.Api.Controllers
{
    [RoutePrefix("QaStatus")]
    public class QaStatusController : ApiController
    {
        public IImplementQueryAction<GetQaStatusRequest, GetQaStatusResponse> GetQaStatusAction { get; set; }

        [HttpGet]
        [Route("{packageName}")]
        public GetQaStatusResponse GetQaStatus(string packageName)
        {
            var request = new GetQaStatusRequest
            {
                PackageName = packageName
            };

            return GetQaStatusAction.Handle(ActionContext, request);
        }

    }
}
