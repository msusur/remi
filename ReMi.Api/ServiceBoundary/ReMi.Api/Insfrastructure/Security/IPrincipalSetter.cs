using System.Security.Principal;
using System.Web.Http.Controllers;

namespace ReMi.Api.Insfrastructure.Security
{
    public interface IPrincipalSetter
    {
        void SetPrincipal(IPrincipal principal, HttpActionContext actionContext);
    }
}
