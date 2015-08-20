using System.Web.Http;
using ReMi.Common.WebApi;
using ReMi.Queries.Configuration;

namespace ReMi.Api.Controllers
{
    [RoutePrefix("configuration")]
    public class ConfigurationController : ApiController
    {
        public IImplementQueryAction<GetProductsRequest, GetProductsResponse> GetProductsAction { get; set; }
        public IImplementQueryAction<GetCommandsWithRolesRequest, GetCommandsWithRolesResponse> GetCommandsAction { get; set; }
        public IImplementQueryAction<GetCommandsByNamesRequest, GetCommandsByNamesResponse> GetCommandsByNamesAction { get; set; }
        public IImplementQueryAction<GetQueriesWithRolesRequest, GetQueriesWithRolesResponse> GetQueriesAction { get; set; }
        public IImplementQueryAction<GetBusinessUnitsRequest, GetBusinessUnitsResponse> GetBusinessUnitsAction { get; set; }

        [HttpGet]
        [Route("businessUnits/{includeAll?}")]
        public GetBusinessUnitsResponse GetBusinessUnits(bool includeAll = false)
        {
            var request = new GetBusinessUnitsRequest
            {
                IncludeAll = includeAll
            };

            return GetBusinessUnitsAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("products")]
        public GetProductsResponse GetProducts()
        {
            var request = new GetProductsRequest();

            return GetProductsAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("commands")]
        public GetCommandsWithRolesResponse GetCommands()
        {
            var request = new GetCommandsWithRolesRequest();

            return GetCommandsAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("commands/{names}")]
        public GetCommandsByNamesResponse GetCommandByName(string names)
        {
            var request = new GetCommandsByNamesRequest
            {
                Names = names
            };

            return GetCommandsByNamesAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("queries")]
        public GetQueriesWithRolesResponse GetQueries()
        {
            var request = new GetQueriesWithRolesRequest();

            return GetQueriesAction.Handle(ActionContext, request);
        }
    }
}
