using ReMi.DataEntities.BusinessRules;
using System.Data.Entity.ModelConfiguration;

namespace ReMi.DataEntityMaps.BusinessRules
{
    public class BusinessRuleDescriptionMap : EntityTypeConfiguration<BusinessRuleDescription>
    {
        public BusinessRuleDescriptionMap()
        {
            HasMany(x => x.Parameters)
                .WithRequired(x => x.BusinessRule)
                .HasForeignKey(x => x.BusinessRuleId);

            HasRequired(x => x.AccountTestData)
                .WithRequiredDependent()
                .WillCascadeOnDelete(true);
        }
    }
}
