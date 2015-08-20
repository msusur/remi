using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace ReMi.Common.WebApi.ApiControllers
{
    public static class ApiControllerExtensions
    {
        public static AcceptedActionResult Accepted(this ApiController controller)
        {
            return new AcceptedActionResult(controller.Request);
        }
        public static AcceptedActionResult Accepted(this ApiController controller, Guid commandId)
        {
            return new AcceptedActionResult(controller.Request, commandId);
        }
    }
}
