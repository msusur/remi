using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace ReMi.Common.WebApi.ApiControllers
{
    public class AcceptedActionResult : IHttpActionResult
    {
        public AcceptedActionResult(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            Request = request;
        }

        public AcceptedActionResult(HttpRequestMessage request, Guid commandId)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            Request = request;
            CommandId = commandId;
        }

        public HttpRequestMessage Request { get; private set; }
        public Guid CommandId { get; private set; }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        public HttpResponseMessage Execute()
        {
            if (CommandId == Guid.Empty)
                return Request.CreateResponse(HttpStatusCode.Accepted);

            return Request.CreateResponse(HttpStatusCode.Accepted, new { CommandId }, new JsonMediaTypeFormatter(), new MediaTypeHeaderValue("application/json"));
        }
    }
}
