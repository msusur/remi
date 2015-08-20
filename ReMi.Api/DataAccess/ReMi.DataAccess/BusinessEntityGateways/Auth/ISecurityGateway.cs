using System;
using ReMi.BusinessEntities.Auth;
using System.Collections.Generic;

namespace ReMi.DataAccess.BusinessEntityGateways.Auth
{
    public interface ISecurityGateway : IDisposable
    {
        IEnumerable<Role> GetCommandRoles(string commandName);
        IEnumerable<Role> GetQueryRoles(string queryName);
    }
}
