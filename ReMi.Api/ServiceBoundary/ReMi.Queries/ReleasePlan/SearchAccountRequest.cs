using System;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.ReleasePlan
{
    [Query("Get users", QueryGroup.AccessControl)]
    public class SearchAccountRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public string Criteria { get; set; }

        public override string ToString()
        {
            return String.Format("Criteria={0}", Criteria);
        }
    }
}
