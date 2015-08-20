using System;
using ReMi.BusinessEntities.Auth;

namespace ReMi.BusinessEntities.BusinessRules
{
    public class BusinessRuleAccountTestData
    {
        public Guid ExternalId { get; set; }
        public string JsonData { get; set; }

        public override string ToString()
        {
            return String.Format("[ExternalId={0}, Account={1}]",
                ExternalId, JsonData);
        }
    }
}
