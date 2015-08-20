using System;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.Auth
{
    [Query("Get active session", QueryGroup.AccessControl, IsStatic = true)]
    public class GetActiveSessionRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public string UserName { get; set; }

        public Guid SessionId { get; set; }

        public override string ToString()
        {
            return string.Format("[UserName = {0}, SessionId = {1}]", UserName, SessionId);
        }
    }
}
