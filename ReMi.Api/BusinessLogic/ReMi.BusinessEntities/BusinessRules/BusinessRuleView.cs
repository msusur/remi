using ReMi.Common.Constants.BusinessRules;
using System;

namespace ReMi.BusinessEntities.BusinessRules
{
    public class BusinessRuleView
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid ExternalId { get; set; }
        public BusinessRuleGroup Group { get; set; }
        public string CodeBeggining { get; set; }

        public override string ToString()
        {
            return String.Format("[Name={0}, Description={1}, ExternalId={2}, Group={3}, CodeBeggining={4}]",
                Name, Description, ExternalId, Group, CodeBeggining);
        }
    }
}
