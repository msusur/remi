using ReMi.BusinessEntities.Api;
using System;
using System.Collections.Generic;

namespace ReMi.DataAccess.BusinessEntityGateways.Auth
{
    public interface IQueryPermissionsGateway : IDisposable
    {
        IEnumerable<Query> GetQueries(bool includeBackground = false);
        IEnumerable<String> GetAllowedQueries(Guid? roleId);

        void AddQueryPermission(int queryId, Guid roleExternalId);
        void RemoveQueryPermission(int queryId, Guid roleExternalId);
    }
}
