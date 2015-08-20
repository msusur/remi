using System.Data.Entity.Migrations;
using System.Web.Http;
using ReMi.Plugin.Common.PluginsConfiguration;
using ReMi.DataAccess.Migrations;

namespace ReMi.Api
{
    public static class DatabaseUpdater
    {
        public static void InitialiseDatabases(HttpConfiguration configuration)
        {
            var configurationDb = new Configuration();

            var migrator = new DbMigrator(configurationDb);
            migrator.Update();

            var pluginsConfiguration = (IPluginConfiguration)configuration.DependencyResolver.GetService(typeof (IPluginConfiguration));
            pluginsConfiguration.InitializeDatabase();
        }
    }
}
