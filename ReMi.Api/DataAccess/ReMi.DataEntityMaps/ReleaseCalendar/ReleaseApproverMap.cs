using System.Data.Entity.ModelConfiguration;
using ReMi.DataEntities;
using ReMi.DataEntities.ReleasePlan;

namespace ReMi.DataEntityMaps.ReleaseCalendar
{
    public class ReleaseApproverMap : EntityTypeConfiguration<ReleaseApprover>
    {
        public ReleaseApproverMap()
        {
            HasKey(e => e.ReleaseApproverId);

            
            Property(e => e.ReleaseApproverId).HasColumnName("ReleaseApproverId");

            Property(e => e.ExternalId).HasColumnName("ExternalId").IsRequired();

            Property(e => e.ApprovedOn).HasColumnName("ApprovedOn").IsOptional();

            Property(e => e.CreatedOn).HasColumnName("CreatedOn").IsRequired();

            ToTable("ReleaseApprovers", Constants.SchemaName);
        }
    }
}
