using System.Data.Entity;
using ReMi.Plugin.Gerrit.DataAccess.DataEntities;
using ReMi.Plugin.Gerrit.DataAccess.Setup;
using ReMi.Plugin.Gerrit.Migrations;

namespace ReMi.Plugin.Gerrit.DataAccess
{
    public class GerritContext : DbContext
    {
        public GerritContext()
            : base(Constants.ConnectionString)
        { }
        public GerritContext(string connectionString)
            : base(connectionString)
        { }

        public DbSet<GlobalConfiguration> GlobalConfigurations { get; set; }
        public DbSet<PackageConfiguration> PackageConfiguration { get; set; }
    }
}
