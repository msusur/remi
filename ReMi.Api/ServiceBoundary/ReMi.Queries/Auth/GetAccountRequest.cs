using System;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.Auth
{
    [Query("Get account", QueryGroup.AccessControl)]
    public class GetAccountRequest : IQuery
    {
        public QueryContext Context { get; set; }
        public Guid AccountId { get; set; }

        public override string ToString()
        {
            return string.Format("[AccountId={0}, QueryContext={1}]", AccountId, Context);
        }
    }
}
