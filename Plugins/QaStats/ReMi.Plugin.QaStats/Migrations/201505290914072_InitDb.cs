namespace ReMi.Plugin.QaStats.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class InitDb : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "QaStats.GlobalConfiguration",
                c => new
                    {
                        GlobalConfigurationId = c.Int(nullable: false, identity: true),
                        QaServiceUrl = c.String(maxLength: 1024),
                    })
                .PrimaryKey(t => t.GlobalConfigurationId);
            
            CreateTable(
                "QaStats.PackageConfiguration",
                c => new
                    {
                        PackageConfigurationId = c.Int(nullable: false, identity: true),
                        PackageId = c.Guid(nullable: false),
                        PackagePath = c.String(maxLength: 1024),
                    })
                .PrimaryKey(t => t.PackageConfigurationId);
            
        }
        
        public override void Down()
        {
            DropTable("QaStats.PackageConfiguration");
            DropTable("QaStats.GlobalConfiguration");
        }
    }
}
