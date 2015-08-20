using System;
using AutoMapper;
using ReMi.BusinessEntities.Api;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.Auth;
using System.Collections.Generic;
using System.Linq;
using ReMi.Common.Utils.Repository;
using DataQuery = ReMi.DataEntities.Api.Query;

namespace ReMi.DataAccess.BusinessEntityGateways.Auth
{
    public class QueryPermissionsGateway : BaseGateway, IQueryPermissionsGateway
    {
        public IRepository<DataQuery> QueryRepository { get; set; }
        public IRepository<Role> RoleRepository { get; set; }
        public IRepository<QueryPermission> QueryPermissionRepository { get; set; }
        public IMappingEngine Mapper { get; set; }

        public IEnumerable<Query> GetQueries(bool includeStatic = false)
        {
            return QueryRepository.GetAllSatisfiedBy(x => !x.IsStatic || includeStatic)
                .Select(x => Mapper.Map<DataQuery, Query>(x))
                .OrderBy(x => x.Group)
                .ThenBy(x => x.Name)
                .ToArray();
        }

        public IEnumerable<string> GetAllowedQueries(Guid roleId)
        {
            var role = RoleRepository.GetSatisfiedBy(x => x.ExternalId == roleId);
            if (role == null)
                throw new EntityNotFoundException(typeof(Role), roleId);
            return role.Name == "Admin"
                ? QueryRepository.Entities.Select(x => x.Name).ToArray()
                : QueryPermissionRepository.GetAllSatisfiedBy(x => x.Role.ExternalId == roleId)
                    .Select(s => s.Query.Name);
        }

        public void AddQueryPermission(int queryId, Guid roleExternalId)
        {
            var query = QueryRepository.GetSatisfiedBy(x => x.QueryId == queryId);
            if (query == null)
                throw new QueryNotFoundException(queryId);

            var role = RoleRepository.GetSatisfiedBy(x => x.ExternalId == roleExternalId);
            if (role == null)
                throw new RoleNotFoundException(roleExternalId);

            if (query.QueryPermissions == null || query.QueryPermissions.Any(x => x.RoleId == role.Id))
                return;

            query.QueryPermissions.Add(new QueryPermission
            {
                QueryId = query.QueryId,
                RoleId = role.Id
            });
            QueryRepository.Update(query);
        }

        public void RemoveQueryPermission(int queryId, Guid roleExternalId)
        {
            var query = QueryRepository.GetSatisfiedBy(x => x.QueryId == queryId);
            if (query == null)
                throw new QueryNotFoundException(queryId);

            var role = RoleRepository.GetSatisfiedBy(x => x.ExternalId == roleExternalId);
            if (role == null)
                throw new RoleNotFoundException(roleExternalId);

            if (query.QueryPermissions == null || query.QueryPermissions.All(x => x.RoleId != role.Id))
                return;

            QueryPermissionRepository.Delete(QueryPermissionRepository
                .GetSatisfiedBy(x => x.RoleId == role.Id && x.QueryId == queryId));
        }
    }
}
