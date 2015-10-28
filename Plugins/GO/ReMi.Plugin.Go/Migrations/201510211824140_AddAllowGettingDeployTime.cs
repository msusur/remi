namespace ReMi.Plugin.Go.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddAllowGettingDeployTime : DbMigration
    {
        public override void Up()
        {
            AddColumn("Go.PackageConfiguration", "AllowGettingDeployTime", c => c.Boolean(false));
        }
        
        public override void Down()
        {
            DropColumn("Go.PackageConfiguration", "AllowGettingDeployTime");
        }
    }
}
