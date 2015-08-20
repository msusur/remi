using System.Data.Entity.ModelConfiguration;
using ReMi.DataEntities;
using ReMi.DataEntities.ReleasePlan;

namespace ReMi.DataEntityMaps.ReleasePlan
{
    public class CheckListQuestionsToProductsMap : EntityTypeConfiguration<CheckListQuestionToProduct>
    {
        public CheckListQuestionsToProductsMap()
        {
            HasKey(t => t.CheckListQuestionsToProductsId);

            ToTable("CheckListQuestionsToProducts", Constants.SchemaName);
            Property(t => t.ProductId).HasColumnName("ProductId");
            Property(t => t.CheckListQuestionId).HasColumnName("CheckListQuestionId");
        }
    }
}
