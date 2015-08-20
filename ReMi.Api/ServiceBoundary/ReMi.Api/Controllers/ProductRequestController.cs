using System.Web.Http;
using ReMi.Common.WebApi;
using ReMi.Queries.ProductRequests;

namespace ReMi.Api.Controllers
{
    [RoutePrefix("productRequests")]
    public class ProductRequestController : ApiController
    {
        public IImplementQueryAction<GetProductRequestsRequest, GetProductRequestsResponse>
            GetProductRequestAction { get; set; }

        public IImplementQueryAction<GetProductRequestRegistrationsRequest, GetProductRequestRegistrationsResponse>
            GetRegistrationsAction { get; set; }

        [Route("config")]
        public GetProductRequestsResponse GetConfigurations()
        {
            var request = new GetProductRequestsRequest();

            return GetProductRequestAction.Handle(ActionContext, request);
        }

        [Route("registrations")]
        public GetProductRequestRegistrationsResponse GetRegistrations()
        {
            var request = new GetProductRequestRegistrationsRequest();

            return GetRegistrationsAction.Handle(ActionContext, request);
        }
    }
}
