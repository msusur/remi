using System.Data.Entity.Migrations;
using ReMi.Api.Runner.Migrations;

namespace ReMi.Api.Runner
{
    public class WebApiApplication : Api.WebApiApplication
    {
        protected override void Application_Start()
        {
            base.Application_Start();

            var configurationDb = new Configuration();

            var migrator = new DbMigrator(configurationDb);
            migrator.Update();
        }
    }
}
