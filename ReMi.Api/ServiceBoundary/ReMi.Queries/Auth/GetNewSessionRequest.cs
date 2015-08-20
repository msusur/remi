using System;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.Auth
{
    [Query("Get new session", QueryGroup.AccessControl, IsStatic = true)]
    public class GetNewSessionRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public Guid SessionId { get; set; }

        public override string ToString()
        {
            return string.Format("SessionId = {0}",
                SessionId);
        }
    }
}
