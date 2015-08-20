using System;
using ReMi.Common.Constants.BusinessRules;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.BusinessRules
{
    [Query("Get Business Rule", QueryGroup.BusinessRules)]
    public class GetBusinessRuleRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public BusinessRuleGroup Group { get; set; }
        public string Name { get; set; }
        public Guid? ExternalId { get; set; }

        public override string ToString()
        {
            return string.Format("[Group={0}, Name={1}, ExternalId={2}]", Group, Name, ExternalId);
        }
    }
}
