using ReMi.Common.WebApi;
using ReMi.Queries.Metrics;
using System;
using System.Web.Http;
using ReMi.Queries.ReleasePlan;

namespace ReMi.Api.Controllers
{
    [RoutePrefix("metrics")]
    public class MetricsController : ApiController
    {
        public IImplementQueryAction<GetMetricsRequest, GetMetricsResponse> GetMetricsAction { get; set; }

        #region GET

        [HttpGet]
        [Route("release/execution/{releaseWindowId:guid}")]
        public GetMetricsResponse GetMetrics(Guid releaseWindowId)
        {
            var request = new GetMetricsRequest
            {
                ReleaseWindowId = releaseWindowId
            };

            return GetMetricsAction.Handle(ActionContext, request);
        }

        

        #endregion
    }
}
