namespace ReMi.Plugin.Jenkins.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddTimeZoneConfiguration : DbMigration
    {
        public override void Up()
        {
            AddColumn("Jenkins.PackageConfiguration", "TimeZone", c => c.Int(nullable: true));
            Sql(@"UPDATE Jenkins.PackageConfiguration SET TimeZone = 1");
            AlterColumn("Jenkins.PackageConfiguration", "TimeZone", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Jenkins.PackageConfiguration", "TimeZone");
        }
    }
}
