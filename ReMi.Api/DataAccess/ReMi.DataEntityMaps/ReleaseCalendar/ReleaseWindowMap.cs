using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using ReMi.DataEntities;
using ReMi.DataEntities.ReleaseCalendar;

namespace ReMi.DataEntityMaps.ReleaseCalendar
{
    public class ReleaseWindowMap : EntityTypeConfiguration<ReleaseWindow>
    {
        public ReleaseWindowMap()
        {
            // Primary Key 
            HasKey(t => t.ReleaseWindowId);

            // Properties 
            Property(t => t.StartTime)
                .IsRequired();

            Property(t => t.OriginalStartTime)
                .IsRequired();

            Property(t => t.RequiresDowntime)
                .IsRequired();

            Property(t => t.ReleaseType)
                .IsRequired();

            Property(t => t.CreatedOn)
                .IsRequired();

            Property(t => t.ExternalId)
                .IsRequired();

            HasMany(x => x.CheckList).WithRequired(x => x.ReleaseWindow);

            HasMany(x => x.ReleaseTasks).WithRequired(x => x.ReleaseWindow);

            HasRequired(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => x.CreatedById)
                .WillCascadeOnDelete(false);

            // Table & Column Mappings 
            ToTable("ReleaseWindows", Constants.SchemaName);
            Property(t => t.ReleaseWindowId).HasColumnName("ReleaseWindowId");
            Property(t => t.StartTime).HasColumnName("StartTime");
            Property(t => t.OriginalStartTime).HasColumnName("OriginalStartTime");
            Property(t => t.RequiresDowntime).HasColumnName("RequiresDowntime");
            Property(t => t.CreatedOn).HasColumnName("CreatedOn");
            Property(t => t.ReleaseType).HasColumnName("ReleaseTypeId");
            Property(t => t.ExternalId).HasColumnName("ExternalId");
        }

    }
}
