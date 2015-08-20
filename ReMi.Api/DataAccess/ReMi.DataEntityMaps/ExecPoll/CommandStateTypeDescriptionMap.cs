using System.Data.Entity.ModelConfiguration;
using ReMi.DataEntities;
using ReMi.DataEntities.ExecPoll;

namespace ReMi.DataEntityMaps.ExecPoll
{
    public class CommandStateTypeDescriptionMap : EntityTypeConfiguration<CommandStateTypeDescription>
    {
        public CommandStateTypeDescriptionMap()
        {
            // Primary Key 
            HasKey(t => t.CommandStateTypeId);

            // Properties 
            Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(128)
                .IsUnicode(false);

            // Table & Column Mappings 
            ToTable("CommandStateTypes", Constants.ExecPollSchemaName);
            Property(t => t.CommandStateTypeId).HasColumnName("CommandStateTypeId");
            Property(t => t.Description).HasColumnName("Description");
        }
    }
}
