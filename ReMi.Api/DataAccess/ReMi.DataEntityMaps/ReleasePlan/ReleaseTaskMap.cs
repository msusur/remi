using System.ComponentModel.DataAnnotations.Schema;
using ReMi.DataEntities;
using ReMi.DataEntities.ReleasePlan;
using System.Data.Entity.ModelConfiguration;

namespace ReMi.DataEntityMaps.ReleasePlan
{
    public class ReleaseTaskMap : EntityTypeConfiguration<ReleaseTask>
    {
        public ReleaseTaskMap()
        {
            HasKey(x => x.ReleaseTaskId);

            Property(x => x.ExternalId)
                .IsRequired();

            Property(x => x.Description)
                .HasMaxLength(512)
                .IsRequired();

            Property(x => x.ReleaseWindowId)
                .IsRequired();

            Property(x => x.Type)
                .IsRequired();

            Property(x => x.HelpDeskReference)
                .IsOptional();

            Property(x => x.HelpDeskUrl)
                .IsOptional();

            HasMany(x => x.Attachments)
                .WithOptional()
                .HasForeignKey(x => x.ReleaseTaskId);

            Property(x => x.AssigneeAccountId)
                .IsRequired();

            Property(x => x.CreatedByAccountId)
                .IsRequired();

            Property(t => t.CreatedOn)
                .IsRequired();

            Property(t => t.CompletedOn)
                .IsOptional();

            Property(x => x.ReceiptConfirmedOn).IsOptional();

            HasRequired(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => x.CreatedByAccountId)
                .WillCascadeOnDelete(false);

            ToTable("ReleaseTask", Constants.SchemaName);
        }
    }
}
