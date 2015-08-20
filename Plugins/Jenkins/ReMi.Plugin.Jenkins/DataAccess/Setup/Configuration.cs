using System.Data.Entity.Migrations;
using ReMi.Plugin.Jenkins.DataAccess;
using ReMi.Plugin.Jenkins.DataAccess.Setup;

namespace ReMi.Plugin.Jenkins.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<JenkinsContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = Constants.ContextKey;
            SetHistoryContextFactory("System.Data.SqlClient",
                (conn, schema) => new JenkinsMigrationHistoryContext(conn));
        }

        protected override void Seed(JenkinsContext context)
        {
            
        }
    }
}
