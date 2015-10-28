namespace ReMi.Plugin.Jenkins.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddAllowGettingDeployTime : DbMigration
    {
        public override void Up()
        {
            AddColumn("Jenkins.PackageConfiguration", "AllowGettingDeployTime", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("Jenkins.PackageConfiguration", "AllowGettingDeployTime");
        }
    }
}
