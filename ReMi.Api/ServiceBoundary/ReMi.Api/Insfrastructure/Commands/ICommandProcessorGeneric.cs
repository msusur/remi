using System.Net.Http;
using System.Web.Http.Controllers;
using ReMi.Common.Cqrs;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Api.Insfrastructure.Commands
{
    public interface ICommandProcessorGeneric<in TCommand>  where TCommand : class, ICommand
    {
        HttpResponseMessage HandleRequest(HttpActionContext actionContext, TCommand command);
    }
}
