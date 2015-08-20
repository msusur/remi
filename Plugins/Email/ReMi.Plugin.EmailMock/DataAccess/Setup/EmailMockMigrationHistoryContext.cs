using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Migrations.History;
using ReMi.Plugin.Ldap.DataAccess.Setup;

namespace ReMi.Plugin.EmailMock.DataAccess.Setup
{
    public class EmailMockMigrationHistoryContext : HistoryContext
    {
        public EmailMockMigrationHistoryContext(DbConnection existingConnection) :
            base(existingConnection, Constants.Schema)
        {
            
        }

        public EmailMockMigrationHistoryContext(DbConnection existingConnection, string defaultSchema) : base(existingConnection, defaultSchema)
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
