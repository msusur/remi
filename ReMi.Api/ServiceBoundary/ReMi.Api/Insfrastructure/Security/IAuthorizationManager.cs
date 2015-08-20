using System.Collections.Generic;
using System.Web.Http.Controllers;
using ReMi.BusinessEntities.Auth;

namespace ReMi.Api.Insfrastructure.Security
{
    public interface IAuthorizationManager
    {
        Account Authenticate(HttpActionContext actionContext);
        bool IsAuthorized(IEnumerable<Role> roles);
    }
}
