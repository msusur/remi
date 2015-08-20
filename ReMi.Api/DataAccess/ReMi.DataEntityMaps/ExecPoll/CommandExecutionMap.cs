using System.Data.Entity.ModelConfiguration;
using ReMi.DataEntities;
using ReMi.DataEntities.ExecPoll;

namespace ReMi.DataEntityMaps.ExecPoll
{
    public class CommandExecutionMap : EntityTypeConfiguration<CommandExecution>
    {
        public CommandExecutionMap()
        {
            // Primary Key 
            HasKey(t => t.CommandExecutionId);

            // Properties 
            Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(128)
                .IsUnicode(false);

            Property(t => t.ExternalId)
                .IsRequired();

            Property(t => t.CreatedOn)
                .IsRequired();

            // Table & Column Mappings 
            ToTable("CommandExecutions", Constants.ExecPollSchemaName);
            Property(t => t.CommandExecutionId).HasColumnName("CommandExecutionId");
            Property(t => t.ExternalId).HasColumnName("ExternalId");
            Property(t => t.Description).HasColumnName("Description");
            Property(t => t.CreatedOn).HasColumnName("CreatedOn");
        }
    }
}
