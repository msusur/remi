using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using ReMi.DataEntities;
using ReMi.DataEntities.ReleasePlan;

namespace ReMi.DataEntityMaps.ReleasePlan
{
    public class CheckListMap : EntityTypeConfiguration<CheckList>
    {
        public CheckListMap()
        {
            // Primary Key 
            HasKey(t => t.CheckListId);

            // Properties 
            Property(t => t.Comment)
                .IsOptional()
                .IsUnicode()
                .HasMaxLength(4000);

            this.Property(t => t.ExternalId)
                .IsRequired();

            this.Property(t => t.CheckListQuestionId)
                .IsRequired();

            this.Property(t => t.ReleaseWindowId)
                .IsRequired();

            this.Property(t => t.LastChangedBy)
                .IsOptional();

            this.Property(t => t.Checked)
                .IsRequired();

            // Table & Column Mappings 
            ToTable("CheckList", Constants.SchemaName);
            Property(t => t.CheckListId).HasColumnName("CheckListId");
            Property(t => t.ExternalId).HasColumnName("ExternalId");
            Property(t => t.Comment).HasColumnName("Comment");
            Property(t => t.Checked).HasColumnName("Checked");
            Property(t => t.CheckListQuestionId).HasColumnName("CheckListQuestionId");
            Property(t => t.ReleaseWindowId).HasColumnName("ReleaseWindowId");
            Property(t => t.LastChangedBy).HasColumnName("LastChangedBy");
        }
    }
}
