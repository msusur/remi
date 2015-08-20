using System.Data.Entity.ModelConfiguration;
using ReMi.DataEntities;
using ReMi.DataEntities.ReleasePlan;

namespace ReMi.DataEntityMaps.ReleasePlan
{
    public class CheckListQuestionMap : EntityTypeConfiguration<CheckListQuestion>
    {
        public CheckListQuestionMap()
        {
            // Primary Key 
            HasKey(q => q.CheckListQuestionId);

            // Properties 
            Property(q => q.Content)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(true);

            // Table & Column Mappings 
            ToTable("CheckListQuestions", Constants.SchemaName);
            Property(q => q.CheckListQuestionId).HasColumnName("CheckListQuestionId");
            Property(q => q.Content).HasColumnName("Content");
        }
    }
}
