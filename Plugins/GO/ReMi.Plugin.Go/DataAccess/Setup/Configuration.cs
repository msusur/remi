using System.Data.Entity.Migrations;
using ReMi.Plugin.Go.DataAccess;
using ReMi.Plugin.Go.DataAccess.Setup;

namespace ReMi.Plugin.Go.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<GoContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = Constants.ContextKey;
            SetHistoryContextFactory("System.Data.SqlClient",
                (conn, schema) => new GoMigrationHistoryContext(conn));
        }

        protected override void Seed(GoContext context)
        {
            
        }
    }
}
