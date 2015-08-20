using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using ReMi.DataEntities;
using ReMi.DataEntities.ReleaseCalendar;
using ReMi.DataEntities.ReleasePlan;

namespace ReMi.DataEntityMaps.ReleasePlan
{
    public class ReleaseTaskRiskDescriptionMap : EntityTypeConfiguration<ReleaseTaskRiskDescription>
    {
        public ReleaseTaskRiskDescriptionMap()
        {
            // Primary Key 
            HasKey(t => t.Id);

            Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(128)
                .IsUnicode(false);

            // Properties 
            Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(128)
                .IsUnicode(false);

            // Table & Column Mappings 
            ToTable("ReleaseTaskRisks", Constants.SchemaName);
            
            Property(t => t.Name)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Name", 1) { IsUnique = true }));

            Property(t => t.Description)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Description") { IsUnique = true }));
        }
    }
}
