using System;
using System.Collections.Generic;
using ReMi.BusinessEntities;
using ReMi.BusinessEntities.Auth;

namespace ReMi.DataAccess.BusinessEntityGateways.Auth
{
    public interface IRoleGateway : IDisposable
    {
        IEnumerable<Role> GetRoles();
        void CreateRole(Role role);
        void UpdateRole(Role role);
        void DeleteRole(Role role);
    }
}
