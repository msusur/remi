using ReMi.Api.Insfrastructure.Commands;
using ReMi.Common.WebApi;
using ReMi.Queries.ExecPoll;
using System;
using System.Net.Http;
using System.Web.Http;

namespace ReMi.Api.Controllers
{
    [RoutePrefix("commands")]
    public class CommandsController : ApiController
    {
        public IImplementQueryAction<GetCommandStateRequest, GetCommandStateResponse> GetCommandStateAction { get; set;}

        public ICommandProcessor CommandProcessor { get; set; }


        [HttpPost]
        [Route("deliver/{commandName}")]
        public HttpResponseMessage Deliver(string commandName, bool isSynchronous = false)
        {
            var response = CommandProcessor.HandleRequest(ActionContext, commandName, isSynchronous);
            response.Content = response.Content ?? new StringContent(string.Empty);
            return response;
        }

        #region GET

        [HttpGet]
        [Route("{id:guid}/state")]
        public GetCommandStateResponse GetCommandState(Guid id)
        {
            var request = new GetCommandStateRequest
            {
                ExternalId = id
            };

            return GetCommandStateAction.Handle(ActionContext, request);
        }

        #endregion
    }
}
