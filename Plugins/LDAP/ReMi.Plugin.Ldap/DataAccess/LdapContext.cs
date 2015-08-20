using System.Data.Entity;
using ReMi.Plugin.Ldap.DataAccess.DataEntities;
using ReMi.Plugin.Ldap.DataAccess.Setup;
using ReMi.Plugin.Ldap.Migrations;

namespace ReMi.Plugin.Ldap.DataAccess
{
    public class LdapContext : DbContext
    {
        public LdapContext()
            : base(Constants.ConnectionString)
        { }
        public LdapContext(string connectionString)
            : base(connectionString)
        { }

        public DbSet<GlobalConfiguration> GlobalConfiguration { get; set; }    }
}
