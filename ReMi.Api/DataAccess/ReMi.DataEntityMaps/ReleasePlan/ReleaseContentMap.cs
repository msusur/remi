using ReMi.DataEntities;
using ReMi.DataEntities.ReleasePlan;
using System.Data.Entity.ModelConfiguration;

namespace ReMi.DataEntityMaps.ReleasePlan
{
    public class ReleaseContentMap : EntityTypeConfiguration<ReleaseContent>
    {
        public ReleaseContentMap()
        {
            HasKey(x => x.ReleaseContentId);

            Property(x => x.Comment)
                .IsOptional()
                .IsUnicode()
                .HasMaxLength(1024);

            Property(x => x.TicketId)
                .IsRequired();

            Property(x => x.TicketKey)
                .IsRequired();

            HasRequired(x => x.LastChangedByAccount)
                .WithMany()
                .HasForeignKey(x => x.LastChangedByAccountId);

            HasOptional(x => x.ReleaseWindow)
                .WithMany(x => x.ReleaseContent)
                .HasForeignKey(x => x.ReleaseWindowsId);

            Property(x => x.IncludeToReleaseNotes).IsRequired();

            ToTable("ReleaseContent", Constants.SchemaName);
        }
    }
}
