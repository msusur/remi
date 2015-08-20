using System.Data.Entity;
using ReMi.Plugin.Jenkins.DataAccess.DataEntities;
using ReMi.Plugin.Jenkins.DataAccess.Setup;
using ReMi.Plugin.Jenkins.Migrations;

namespace ReMi.Plugin.Jenkins.DataAccess
{
    public class JenkinsContext : DbContext
    {
        public JenkinsContext()
            : base(Constants.ConnectionString)
        { }
        public JenkinsContext(string connectionString)
            : base(connectionString)
        { }

        public DbSet<GlobalConfiguration> GlobalConfigurations { get; set; }
        public DbSet<PackageConfiguration> PackageConfiguration { get; set; }
        public DbSet<JenkinsServerConfiguration> JenkinsServerConfiguration { get; set; }
        public DbSet<PackageJenkinsJobConfiguration> PackageJenkinsJobConfiguration { get; set; }
    }
}
