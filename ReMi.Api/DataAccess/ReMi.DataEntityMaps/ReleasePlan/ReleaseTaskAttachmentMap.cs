using System.Data.Entity.ModelConfiguration;
using ReMi.DataEntities;
using ReMi.DataEntities.ReleasePlan;

namespace ReMi.DataEntityMaps.ReleasePlan
{
    public class ReleaseTaskAttachmentMap : EntityTypeConfiguration<ReleaseTaskAttachment>
    {
        public ReleaseTaskAttachmentMap()
        {
            HasKey(x => x.ReleaseTaskAttachmentId);

            Property(x => x.ExternalId).IsRequired();

            Property(x => x.Name).IsRequired().HasMaxLength(255);

            Property(x => x.Attachment).IsRequired();

            Property(x => x.ReleaseTaskId).IsRequired();

            Property(x => x.HelpDeskAttachmentId).IsOptional();

            ToTable("ReleaseTaskAttachment", Constants.SchemaName);
        }
    }
}
