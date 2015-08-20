using ReMi.Plugin.ZenDesk.DataAccess;
using ReMi.Plugin.ZenDesk.DataAccess.Setup;
using System.Data.Entity.Migrations;

namespace ReMi.Plugin.ZenDesk.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<ZenDeskContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = Constants.ContextKey;
            SetHistoryContextFactory("System.Data.SqlClient",
                (conn, schema) => new ZenDeskMigrationHistoryContext(conn));
        }

        protected override void Seed(ZenDeskContext context)
        {
        }
    }
}
