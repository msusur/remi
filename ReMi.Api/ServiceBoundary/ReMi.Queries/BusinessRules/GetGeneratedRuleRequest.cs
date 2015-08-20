using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.BusinessRules
{
    [Query("Generate Command Permission Rule Draft", QueryGroup.BusinessRules)]
    public class GetGeneratedRuleRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public string Name { get; set; }
        public string Namespace { get; set; }

        public override string ToString()
        {
            return string.Format("[Name={0}, Namespace={1}]", Name, Namespace);
        }
    }
}
