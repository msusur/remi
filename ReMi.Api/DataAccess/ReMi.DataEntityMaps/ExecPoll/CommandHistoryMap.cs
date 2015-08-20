using System.Data.Entity.ModelConfiguration;
using ReMi.DataEntities;
using ReMi.DataEntities.ExecPoll;

namespace ReMi.DataEntityMaps.ExecPoll
{
    public class CommandHistoryMap : EntityTypeConfiguration<CommandHistory>
    {
        public CommandHistoryMap()
        {
            // Primary Key 
            HasKey(t => t.CommandHistoryId);

            // Properties 
            Property(t => t.State)
                .IsRequired();

            Property(t => t.CommandExecutionId)
                .IsRequired();

            Property(t => t.CreatedOn)
                .IsRequired();
            //.HasDatabaseGeneratedOption(DatabaseGeneratedOption.);

            // Table & Column Mappings 
            ToTable("CommandHistory", Constants.ExecPollSchemaName);
            Property(t => t.CommandHistoryId).HasColumnName("CommandHistoryId");
            Property(t => t.CommandExecutionId).HasColumnName("CommandExecutionId");
            Property(t => t.State).HasColumnName("State");
            Property(t => t.CreatedOn).HasColumnName("CreatedOn");
        }
    }
}
