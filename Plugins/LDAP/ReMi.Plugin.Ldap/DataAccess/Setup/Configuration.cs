using System.Data.Entity.Migrations;
using ReMi.Plugin.Ldap.DataAccess;
using ReMi.Plugin.Ldap.DataAccess.Setup;

namespace ReMi.Plugin.Ldap.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<LdapContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = Constants.ContextKey;
            SetHistoryContextFactory("System.Data.SqlClient",
                (conn, schema) => new LdapMigrationHistoryContext(conn));
        }
    }
}
