using System;
using System.Collections.Generic;
using ReMi.Common.Constants.BusinessRules;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.BusinessRules
{
    [Query("Trigger Business Rule", QueryGroup.BusinessRules)]
    public class TriggerBusinessRuleRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public BusinessRuleGroup Group { get; set; }

        public string Rule { get; set; }

        public Guid ExternalId { get; set; }

        public IDictionary<string, string> Parameters { get; set; }

        public override string ToString()
        {
            return string.Format("[Group={0}, Rule={1}, ExternalId={2}, Parameters={3}]",
                Group, Rule, ExternalId, Parameters.FormatElements());
        }
    }
}
