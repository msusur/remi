using ReMi.DataEntities;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Migrations.History;

namespace ReMi.DataAccess
{
    public class ReMiMigrationHistoryContext : HistoryContext
    {
        public ReMiMigrationHistoryContext(DbConnection existingConnection) :
            base(existingConnection, Constants.SchemaName)
        {
            
        }

        public ReMiMigrationHistoryContext(DbConnection existingConnection, string defaultSchema)
            : base(existingConnection, defaultSchema)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<HistoryRow>().ToTable(
                tableName: Constants.MigrationHistoryTableName,
                schemaName: Constants.SchemaName);
        }    }
}
