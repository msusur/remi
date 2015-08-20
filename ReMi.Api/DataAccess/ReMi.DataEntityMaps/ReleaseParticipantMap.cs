using System.Data.Entity.ModelConfiguration;
using ReMi.DataEntities;

namespace ReMi.DataEntityMaps
{
    public class ReleaseParticipantMap:EntityTypeConfiguration<ReleaseParticipant>
    {
        public ReleaseParticipantMap()
        {
            HasKey(e => e.ReleaseParticipantId);
            
            Property(e => e.ReleaseParticipantId).HasColumnName("ReleaseParticipantId");
            Property(e => e.ExternalId).HasColumnName("ExternalId").IsRequired();
            Property(e => e.ApprovedOn).HasColumnName("ApprovedOn").IsOptional();

            ToTable("ReleaseParticipant", Constants.SchemaName);
        }
    }
}
