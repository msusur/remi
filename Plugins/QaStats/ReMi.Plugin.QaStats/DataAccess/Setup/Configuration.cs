using System.Data.Entity.Migrations;
using ReMi.Plugin.QaStats.DataAccess;

namespace ReMi.Plugin.QaStats.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<QaStatsContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = Constants.ContextKey;
            SetHistoryContextFactory("System.Data.SqlClient",
                (conn, schema) => new QaStatsMigrationHistoryContext(conn));
        }
    }
}
