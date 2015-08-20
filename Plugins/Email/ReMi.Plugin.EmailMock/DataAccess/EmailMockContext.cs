using System.Data.Entity;
using ReMi.Plugin.EmailMock.DataAccess.DataEntities;
using ReMi.Plugin.EmailMock.Migrations;
using ReMi.Plugin.Ldap.DataAccess.Setup;

namespace ReMi.Plugin.EmailMock.DataAccess
{
    public class EmailMockContext : DbContext
    {
        public EmailMockContext()
            : base(Constants.ConnectionString)
        { }
        public EmailMockContext(string connectionString)
            : base(connectionString)
        { }

        public DbSet<GlobalConfiguration> GlobalConfiguration { get; set; }
    }
}
