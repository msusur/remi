using System.Data.Entity;
using ReMi.Plugin.QaStats.DataAccess.DataEntities;
using ReMi.Plugin.QaStats.Migrations;

namespace ReMi.Plugin.QaStats.DataAccess
{
    public class QaStatsContext : DbContext
    {
        public QaStatsContext()
            : base(Constants.ConnectionString)
        { }
        public QaStatsContext(string connectionString)
            : base(connectionString)
        { }

        public DbSet<PackageConfiguration> PackageConfiguration { get; set; }
        public DbSet<GlobalConfiguration> GlobalConfiguration { get; set; }
    }
}
