using ReMi.Common.WebApi;
using ReMi.Queries.Common;
using System.Web.Http;

namespace ReMi.Api.Controllers
{
    [RoutePrefix("common")]
    public class CommonController : ApiController
    {
        public IImplementQueryAction<GetEnumsRequest, GetEnumsResponse> GetEnumsAction { get; set;}

        [HttpGet]
        [Route("enums")]
        public GetEnumsResponse GetEnums()
        {
            return GetEnumsAction.Handle(ActionContext, new GetEnumsRequest());
        }
    }
}
