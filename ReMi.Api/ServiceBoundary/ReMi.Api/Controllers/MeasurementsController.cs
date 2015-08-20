using System;
using System.Web.Http;
using ReMi.Common.WebApi;
using ReMi.Queries.Metrics;
using ReMi.Queries.ReleaseExecution;

namespace ReMi.Api.Controllers
{
    [RoutePrefix("measurements")]
    public class MeasurementsController : ApiController
    {
        public IImplementQueryAction<GetMeasurementsRequest, GetMeasurementsResponse> GetMeasurementsAction { get; set; }
        public IImplementQueryAction<GetDeploymentMeasurementsRequest, GetDeploymentMeasurementsResponse> GetDeploymentJobMeasurementsAction { get; set; }
        public IImplementQueryAction<GetDeploymentJobMeasurementsByProductRequest, GetDeploymentJobMeasurementsByProductResponse> 
            GetDeploymentJobMeasurementsByProductRequestAction { get; set; }

        [HttpGet]
        [Route("{product}")]
        public GetMeasurementsResponse GetRelease(String product)
        {
            var request = new GetMeasurementsRequest
            {
                Product = product
            };

            return GetMeasurementsAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("deploymentJobs/{releaseWindowId:guid}")]
        public GetDeploymentMeasurementsResponse GetDeploymentJobMeasurements(Guid releaseWindowId)
        {
            var request = new GetDeploymentMeasurementsRequest
            {
                ReleaseWindowId = releaseWindowId
            };
            return GetDeploymentJobMeasurementsAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("jobs/{product}")]
        public GetDeploymentJobMeasurementsByProductResponse GetDeploymentJobMeasurements(string product)
        {
            var request = new GetDeploymentJobMeasurementsByProductRequest
            {
                Product = product
            };
            return GetDeploymentJobMeasurementsByProductRequestAction.Handle(ActionContext, request);
        }
    }
}
