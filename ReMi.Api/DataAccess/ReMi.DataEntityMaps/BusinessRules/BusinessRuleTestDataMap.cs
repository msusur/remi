using ReMi.DataEntities.BusinessRules;
using System.Data.Entity.ModelConfiguration;

namespace ReMi.DataEntityMaps.BusinessRules
{
    public class BusinessRuleTestDataMap : EntityTypeConfiguration<BusinessRuleTestData>
    {
        public BusinessRuleTestDataMap()
        {
            HasRequired(x => x.Parameter)
                .WithRequiredDependent(x => x.TestData)
                .WillCascadeOnDelete(true);
        }
    }
}
