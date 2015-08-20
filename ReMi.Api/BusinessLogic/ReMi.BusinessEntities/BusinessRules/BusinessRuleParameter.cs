using System;

namespace ReMi.BusinessEntities.BusinessRules
{
    public class BusinessRuleParameter
    {
        public Guid ExternalId { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public BusinessRuleTestData TestData { get; set; }

        public override string ToString()
        {
            return String.Format("[ExternalId={0}, Type={1}, Name={2}, TestData={3}]",
                ExternalId, Type, Name, TestData);
        }
    }
}
