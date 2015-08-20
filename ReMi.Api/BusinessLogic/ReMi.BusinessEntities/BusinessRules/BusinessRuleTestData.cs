using System;

namespace ReMi.BusinessEntities.BusinessRules
{
    public class BusinessRuleTestData
    {
        public Guid ExternalId { get; set; }
        public string JsonData { get; set; }

        public override string ToString()
        {
            return String.Format("[ExternalId={0}, JsonData={1}]",
                ExternalId, JsonData);
        }
    }
}
