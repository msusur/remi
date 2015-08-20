using System.Data.Entity.ModelConfiguration;
using ReMi.DataEntities.ReleasePlan;

namespace ReMi.DataEntityMaps.ReleasePlan
{
    public class ReleaseDeploymentMeasurementMap : EntityTypeConfiguration<ReleaseDeploymentMeasurement>
    {
        public ReleaseDeploymentMeasurementMap()
        {
            HasRequired(x => x.ReleaseWindow)
              .WithMany()
              .HasForeignKey(x => x.ReleaseWindowId)
              .WillCascadeOnDelete(true);

            HasRequired(x => x.CreatedBy)
              .WithMany()
              .HasForeignKey(x => x.CreatedByAccountId)
              .WillCascadeOnDelete(false);

            HasOptional(x => x.ParentMeasurement)
              .WithMany()
              .HasForeignKey(x => x.ParentMeasurementId)
              .WillCascadeOnDelete(false);
        }
    }
}
