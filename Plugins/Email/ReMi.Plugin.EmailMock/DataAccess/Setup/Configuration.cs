using System.Data.Entity.Migrations;
using ReMi.Plugin.EmailMock.DataAccess;
using ReMi.Plugin.EmailMock.DataAccess.Setup;
using ReMi.Plugin.Ldap.DataAccess;
using ReMi.Plugin.Ldap.DataAccess.Setup;

namespace ReMi.Plugin.EmailMock.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<EmailMockContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = Constants.ContextKey;
            SetHistoryContextFactory("System.Data.SqlClient",
                (conn, schema) => new EmailMockMigrationHistoryContext(conn));
        }
    }
}
