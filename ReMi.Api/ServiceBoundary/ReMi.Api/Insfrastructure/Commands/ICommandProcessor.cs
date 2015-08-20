using System.Net.Http;
using System.Web.Http.Controllers;

namespace ReMi.Api.Insfrastructure.Commands
{
    public interface ICommandProcessor
    {
        HttpResponseMessage HandleRequest(HttpActionContext actionContext, string commandName, bool isSynchronous);
    }
}
