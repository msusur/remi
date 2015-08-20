using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Migrations.History;

namespace ReMi.Plugin.QaStats.DataAccess
{
    public class QaStatsMigrationHistoryContext : HistoryContext
    {
        public QaStatsMigrationHistoryContext(DbConnection existingConnection) :
            base(existingConnection, Constants.Schema)
        {
            
        }

        public QaStatsMigrationHistoryContext(DbConnection existingConnection, string defaultSchema) : base(existingConnection, defaultSchema)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<HistoryRow>().ToTable(
                tableName: Constants.MigrationHistoryTable,
                schemaName: Constants.Schema);
        }
    }
}
