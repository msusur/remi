using ReMi.Contracts.Cqrs.Queries;
using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Queries.ExecPoll
{
    [Query("Get command execution state", QueryGroup.Api)]
    public class GetCommandStateRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public Guid ExternalId { get; set; }

        public override string ToString()
        {
            return string.Format("[ExternalId = {0}]", ExternalId);
        }
    }
}
