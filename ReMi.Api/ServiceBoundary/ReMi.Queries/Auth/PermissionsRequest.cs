using System;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.Auth
{
    [Query("Get permissions for role", QueryGroup.AccessControl)]
    public class PermissionsRequest : IQuery
    {
        public QueryContext Context { get; set; }
        public Guid RoleId { get; set; }

        public override string ToString()
        {
            return String.Format("[RoleId={0}, Context={1}]", RoleId, Context);
        }
    }
}
