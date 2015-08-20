using System;
using System.Collections.Generic;
using ReMi.Common.Constants.BusinessRules;
using ReMi.Common.Utils;

namespace ReMi.BusinessEntities.BusinessRules
{
    public class BusinessRuleDescription
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid ExternalId { get; set; }
        public BusinessRuleGroup Group { get; set; }
        public string Script { get; set; }
        public IEnumerable<BusinessRuleParameter> Parameters { get; set; }
        public BusinessRuleAccountTestData AccountTestData { get; set; }

        public override string ToString()
        {
            return String.Format("[Name={0}, Description={1}, ExternalId={2}, Group={3}, Script={4}, Parameters={5}]",
                Name, Description, ExternalId, Group, Script, Parameters.FormatElements());
        }
    }
}
