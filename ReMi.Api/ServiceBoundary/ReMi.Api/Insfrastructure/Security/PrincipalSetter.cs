using System.Security.Principal;
using System.Threading;
using System.Web.Http.Controllers;

namespace ReMi.Api.Insfrastructure.Security
{
    public class PrincipalSetter : IPrincipalSetter
    {
        public void SetPrincipal(IPrincipal principal, HttpActionContext actionContext)
        {
            Thread.CurrentPrincipal = principal;

            actionContext.RequestContext.Principal = principal;
        }
    }
}
