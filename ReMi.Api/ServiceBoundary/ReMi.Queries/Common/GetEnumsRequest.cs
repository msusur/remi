using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.Common
{
    [Query("Get enums", QueryGroup.Common)]
    public class GetEnumsRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public override string ToString()
        {
            return string.Format("[Context={0}]", Context);
        }
    }
}
