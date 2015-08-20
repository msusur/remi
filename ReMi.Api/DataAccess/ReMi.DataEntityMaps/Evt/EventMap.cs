using System.Data.Entity.ModelConfiguration;
using ReMi.DataEntities;
using ReMi.DataEntities.Evt;

namespace ReMi.DataEntityMaps.Evt
{
    public class EventMap : EntityTypeConfiguration<Event>
    {
        public EventMap()
        {
            // Primary Key 
            HasKey(t => t.EventId);

            // Properties 
            Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(128)
                .IsUnicode(false);

            Property(t => t.Handler)
                .IsOptional()
                .HasMaxLength(1024)
                .IsUnicode(false);

            Property(t => t.Data)
                .IsOptional()
                .HasMaxLength(4096)
                .IsUnicode(false);

            Property(t => t.ExternalId)
                .IsRequired();

            Property(t => t.CreatedOn)
                .IsRequired();


            // Table & Column Mappings 
            ToTable("Events", Constants.EventSchemaName);
            Property(t => t.EventId).HasColumnName("EventId");
            Property(t => t.ExternalId).HasColumnName("ExternalId");
            Property(t => t.Description).HasColumnName("Description");
            Property(t => t.Data).HasColumnName("Data");
            Property(t => t.Handler).HasColumnName("Handler");
            Property(t => t.CreatedOn).HasColumnName("CreatedOn");
        }
    }
}
