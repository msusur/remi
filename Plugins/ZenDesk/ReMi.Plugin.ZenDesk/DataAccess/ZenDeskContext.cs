using System.Data.Entity;
using ReMi.Plugin.ZenDesk.DataAccess.DataEntities;
using ReMi.Plugin.ZenDesk.DataAccess.Setup;
using ReMi.Plugin.ZenDesk.Migrations;

namespace ReMi.Plugin.ZenDesk.DataAccess
{
    public class ZenDeskContext : DbContext
    {
        public ZenDeskContext()
            : base(Constants.ConnectionString)
        { }
        public ZenDeskContext(string connectionString)
            : base(connectionString)
        { }

        public DbSet<GlobalConfiguration> GlobalConfigurations { get; set; }
        //public DbSet<PackageConfiguration> PackageConfiguration { get; set; }
    }
}
