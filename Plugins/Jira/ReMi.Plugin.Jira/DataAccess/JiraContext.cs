using System.Data.Entity;
using ReMi.Plugin.Jira.DataAccess.DataEntities;
using ReMi.Plugin.Jira.DataAccess.Setup;
using ReMi.Plugin.Jira.Migrations;

namespace ReMi.Plugin.Jira.DataAccess
{
    public class JiraContext : DbContext
    {
        public JiraContext()
            : base(Constants.ConnectionString)
        { }
        public JiraContext(string connectionString)
            : base(connectionString)
        { }

        public DbSet<GlobalConfiguration> GlobalConfigurations { get; set; }
        public DbSet<PackageConfiguration> PackageConfiguration { get; set; }
        public DbSet<PackageJqlFilter> PackageJqlFilters { get; set; }
        public DbSet<PackageDefectJqlFilter> PackageDefectJqlFilters { get; set; }
        public DbSet<UpdateTicketModeDescription> UpdateTicketModes { get; set; }
    }
}
