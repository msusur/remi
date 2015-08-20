using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Migrations.History;

namespace ReMi.Plugin.Gerrit.DataAccess.Setup
{
    public class GerritMigrationHistoryContext : HistoryContext
    {
        public GerritMigrationHistoryContext(DbConnection existingConnection) :
            base(existingConnection, Constants.Schema)
        {
            
        }

        public GerritMigrationHistoryContext(DbConnection existingConnection, string defaultSchema)
            : base(existingConnection, defaultSchema)
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
