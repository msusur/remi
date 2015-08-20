using System.Data.Entity.ModelConfiguration;
using ReMi.DataEntities;
using ReMi.DataEntities.Evt;

namespace ReMi.DataEntityMaps.Evt
{
    public class EventStateTypeDescriptionMap : EntityTypeConfiguration<EventStateTypeDescription>
    {
        public EventStateTypeDescriptionMap()
        {
            // Primary Key 
            HasKey(t => t.EventStateTypeId);

            // Properties 
            Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(128)
                .IsUnicode(false);

            // Table & Column Mappings 
            ToTable("EventStateTypes", Constants.EventSchemaName);
            Property(t => t.EventStateTypeId).HasColumnName("EventStateTypeId");
            Property(t => t.Description).HasColumnName("Description");
        }
    }
}
