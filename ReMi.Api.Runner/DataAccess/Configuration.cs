using System.Data.Entity.Migrations;
using ReMi.Api.Runner.DataAccess;

namespace ReMi.Api.Runner.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<ReMiRunnerContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = Constants.ContextKey;
            SetHistoryContextFactory("System.Data.SqlClient",
                (conn, schema) => new ReMiRunnerMigrationHistoryContext(conn));
        }
    }
}
