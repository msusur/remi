using System.Data.Entity.Migrations;
using ReMi.Plugin.Gerrit.DataAccess;
using ReMi.Plugin.Gerrit.DataAccess.Setup;

namespace ReMi.Plugin.Gerrit.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<GerritContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = Constants.ContextKey;
            SetHistoryContextFactory("System.Data.SqlClient",
                (conn, schema) => new GerritMigrationHistoryContext(conn));
        }

        protected override void Seed(GerritContext context)
        {
            
        }
    }
}
