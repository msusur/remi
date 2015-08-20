using System;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.ReleaseCalendar
{
    [Query("Get release data", QueryGroup.ReleaseCalendar)]
    public class GetReleaseRequest : IQuery
    {
        public Guid ReleaseWindowId { get; set; }

        public QueryContext Context { get; set; }

        public override string ToString()
        {
            return string.Format("[ReleaseWindowId={0}]", ReleaseWindowId);
        }
    }
}
