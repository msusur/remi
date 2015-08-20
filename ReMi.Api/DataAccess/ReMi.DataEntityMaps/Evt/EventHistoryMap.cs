using System.Data.Entity.ModelConfiguration;
using ReMi.DataEntities;
using ReMi.DataEntities.Evt;

namespace ReMi.DataEntityMaps.Evt
{
    public class EventHistoryMap : EntityTypeConfiguration<EventHistory>
    {
        public EventHistoryMap()
        {
            // Primary Key 
            HasKey(t => t.EventHistoryId);

            // Properties 
            Property(t => t.State)
                .IsRequired();

            Property(t => t.EventId)
                .IsRequired();

            Property(t => t.CreatedOn)
                .IsRequired();

            // Table & Column Mappings 
            ToTable("EventHistory", Constants.EventSchemaName);
            Property(t => t.EventHistoryId).HasColumnName("EventHistoryId");
            Property(t => t.EventId).HasColumnName("EventId");
            Property(t => t.State).HasColumnName("State");
            Property(t => t.CreatedOn).HasColumnName("CreatedOn");
        }
    }
}
