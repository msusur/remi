using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.Configuration
{
    [Query("Get release track", QueryGroup.Configuration, IsStatic = true)]
    public class GetReleaseTrackRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public override string ToString()
        {
            return "[]";
        }
    }
}
