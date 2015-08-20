using System.Web.Http;
using ReMi.Common.WebApi;
using ReMi.Queries.Configuration;
using ReMi.Queries.ReleasePlan;

namespace ReMi.Api.Controllers
{
    [RoutePrefix("dictionaries")]
    public class DictionariesController : ApiController
    {
        public IImplementQueryAction<GetReleaseTaskTypesRequest, GetReleaseTaskTypesResponse> GetReleaseTaskTypesAction { get; set; }
        public IImplementQueryAction<GetReleaseTaskRisksRequest, GetReleaseTaskRisksResponse> GetReleaseTaskRisksAction { get; set; }
        public IImplementQueryAction<GetReleaseTaskEnvironmentsRequest, GetReleaseTaskEnvironmentsResponse> GetReleaseTaskEnvironmentsAction { get; set; }
        public IImplementQueryAction<GetReleaseTrackRequest, GetReleaseTrackResponse> GetReleaseTrackAction { get; set; }

        [Route("releaseTask/types")]
        public GetReleaseTaskTypesResponse GetReleaseTaskTypes()
        {
            var request = new GetReleaseTaskTypesRequest();

            return GetReleaseTaskTypesAction.Handle(ActionContext, request);
        }

        [Route("releaseTask/risks")]
        public GetReleaseTaskRisksResponse GetReleaseTaskRisks()
        {
            var request = new GetReleaseTaskRisksRequest();

            return GetReleaseTaskRisksAction.Handle(ActionContext, request);
        }

        [Route("releaseTask/environments")]
        public GetReleaseTaskEnvironmentsResponse GetReleaseTaskEnvironments()
        {
            var request = new GetReleaseTaskEnvironmentsRequest();
            
            return GetReleaseTaskEnvironmentsAction.Handle(ActionContext, request);
        }

        [Route("product/releaseTracks")]
        public GetReleaseTrackResponse GetReleaseTracks()
        {
            var request = new GetReleaseTrackRequest();

            return GetReleaseTrackAction.Handle(ActionContext, request);
        }
    }
}
