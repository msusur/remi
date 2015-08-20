using System.Data.Entity.ModelConfiguration;
using ReMi.DataEntities.ReleasePlan;

namespace ReMi.DataEntityMaps.ReleasePlan
{
    public class ReleaseJobMap : EntityTypeConfiguration<ReleaseJob>
    {
        public ReleaseJobMap()
        {
            HasRequired(x => x.ReleaseWindow)
                .WithMany(x => x.ReleaseJobs)
                .HasForeignKey(x => x.ReleaseWindowId)
                .WillCascadeOnDelete(true);
        }
    }
}
