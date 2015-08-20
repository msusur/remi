using System.Web.Http.Controllers;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Common.WebApi
{
    public interface IImplementQueryAction<in TRequest, out TResponse> where TRequest : IQuery
    {
        TResponse Handle(HttpActionContext actionContext, TRequest request);
    }
}
