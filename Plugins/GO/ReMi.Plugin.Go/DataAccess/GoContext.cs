using System.Data.Entity;
using ReMi.Plugin.Go.DataAccess.DataEntities;
using ReMi.Plugin.Go.DataAccess.Setup;
using ReMi.Plugin.Go.Migrations;

namespace ReMi.Plugin.Go.DataAccess
{
    public class GoContext : DbContext
    {
        public GoContext()
            : base(Constants.ConnectionString)
        { }
        public GoContext(string connectionString)
            : base(connectionString)
        { }

        public DbSet<GlobalConfiguration> GlobalConfigurations { get; set; }
        public DbSet<PackageConfiguration> PackageConfiguration { get; set; }
        public DbSet<GoServerConfiguration> GoServerConfiguration { get; set; }
        public DbSet<PackageGoPipelineConfiguration> PackageGoPipelineConfiguration { get; set; }
    }
}
