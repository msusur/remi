using System;
using ReMi.Common.WebApi;
using ReMi.Queries.Auth;
using ReMi.Queries.ReleasePlan;
using System.Web.Http;

namespace ReMi.Api.Controllers
{
    [RoutePrefix("accounts")]
    public class AccountController : ApiController
    {
        public IImplementQueryAction<GetAccountsRequest, GetAccountsResponse> GetAccountsAction { get; set; }
        public IImplementQueryAction<GetAccountsByRoleRequest, GetAccountsByRoleResponse> GetAccountsByRoleAction { get;
            set; }
        public IImplementQueryAction<SearchAccountRequest, GetAccountsResponse> SearchAccountAction { get; set; }
        public IImplementQueryAction<GetAccountsByProductRequest, GetAccountsByProductResponse> GetAccountsByProductAction { get; set; }
        public IImplementQueryAction<GetRolesRequest, GetRolesResponse> GetRolesAction { get; set; }
        public IImplementQueryAction<PermissionsRequest, PermissionsResponse> GetPermissionsAction { get; set; }

        #region GET
        
        [HttpGet]
        [Route("product/{product}")]
        public GetAccountsByProductResponse GetAccountsByProduct(string product)
        {
            var request = new GetAccountsByProductRequest { Product = product };

            return GetAccountsByProductAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route]
        public GetAccountsResponse GetAccounts()
        {
            var request = new GetAccountsRequest();

            return GetAccountsAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("roles")]
        public GetRolesResponse GetRoles()
        {
            var request = new GetRolesRequest();

            return GetRolesAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("role/{role}")]
        public GetAccountsByRoleResponse GetAccountsByRole(string role)
        {
            var request = new GetAccountsByRoleRequest { Role = role };

            return GetAccountsByRoleAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("search/{criteria}")]
        public GetAccountsResponse SearchAccounts(string criteria)
        {
            var request = new SearchAccountRequest { Criteria = criteria };

            return SearchAccountAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("permissions/{roleId}")]
        public PermissionsResponse Permissions(Guid roleId)
        {
            var request = new PermissionsRequest { RoleId = roleId };

            return GetPermissionsAction.Handle(ActionContext, request);
        }

        #endregion

    }
}
