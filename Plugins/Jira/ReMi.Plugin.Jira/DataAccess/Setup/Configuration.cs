using System.Data.Entity.Migrations;
using ReMi.Common.Utils.Repository;
using ReMi.Plugin.Jira.DataAccess;
using ReMi.Plugin.Jira.DataAccess.DataEntities;
using ReMi.Plugin.Jira.DataAccess.Setup;

namespace ReMi.Plugin.Jira.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<JiraContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = Constants.ContextKey;
            SetHistoryContextFactory("System.Data.SqlClient",
                (conn, schema) => new JiraMigrationHistoryContext(conn));
        }

        protected override void Seed(JiraContext context)
        {
            context.UpdateTicketModes.AddOrUpdateEnum<UpdateTicketMode, UpdateTicketModeDescription>();
        }
    }
}
