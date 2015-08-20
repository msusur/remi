using ReMi.DataEntities.BusinessRules;
using System.Data.Entity.ModelConfiguration;

namespace ReMi.DataEntityMaps.BusinessRules
{
    public class BusinessRuleAccountTestDataMap : EntityTypeConfiguration<BusinessRuleAccountTestData>
    {
        public BusinessRuleAccountTestDataMap()
        {
            HasRequired(x => x.Rule)
                .WithRequiredDependent(x => x.AccountTestData)
                .WillCascadeOnDelete(true);
        }
    }
}
