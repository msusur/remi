namespace ReMi.Plugin.Go.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class InitializeDatabase : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Go.GlobalConfiguration",
                c => new
                    {
                        GlobalConfigurationId = c.Int(nullable: false, identity: true),
                        GoUser = c.String(),
                        GoPassword = c.String(),
                    })
                .PrimaryKey(t => t.GlobalConfigurationId);
            
            CreateTable(
                "Go.GoServerConfiguration",
                c => new
                    {
                        GoServerConfigurationId = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 256),
                        Value = c.String(),
                        GlobalConfigurationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.GoServerConfigurationId)
                .ForeignKey("Go.GlobalConfiguration", t => t.GlobalConfigurationId, cascadeDelete: true)
                .Index(t => t.Name, unique: true)
                .Index(t => t.GlobalConfigurationId);
            
            CreateTable(
                "Go.PackageConfiguration",
                c => new
                    {
                        PackageConfigurationId = c.Int(nullable: false, identity: true),
                        PackageId = c.Guid(nullable: false),
                        GoServerConfigurationId = c.Int(),
                    })
                .PrimaryKey(t => t.PackageConfigurationId)
                .ForeignKey("Go.GoServerConfiguration", t => t.GoServerConfigurationId)
                .Index(t => t.PackageId, unique: true)
                .Index(t => t.GoServerConfigurationId);
            
            CreateTable(
                "Go.PackageGoPipelineConfiguration",
                c => new
                    {
                        PackageGoPipelineConfigurationId = c.Int(nullable: false, identity: true),
                        ExternalId = c.Guid(nullable: false),
                        Name = c.String(maxLength: 256),
                        IsIncludedByDefault = c.Boolean(nullable: false),
                        IsDisabled = c.Boolean(nullable: false),
                        PackageConfigurationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PackageGoPipelineConfigurationId)
                .ForeignKey("Go.PackageConfiguration", t => t.PackageConfigurationId, cascadeDelete: true)
                .Index(t => t.ExternalId, unique: true)
                .Index(t => new { t.Name, t.PackageConfigurationId }, unique: true, name: "IX_GoPipelineName_PackageConfigurationId");
            
        }
        
        public override void Down()
        {
            DropForeignKey("Go.PackageGoPipelineConfiguration", "PackageConfigurationId", "Go.PackageConfiguration");
            DropForeignKey("Go.PackageConfiguration", "GoServerConfigurationId", "Go.GoServerConfiguration");
            DropForeignKey("Go.GoServerConfiguration", "GlobalConfigurationId", "Go.GlobalConfiguration");
            DropIndex("Go.PackageGoPipelineConfiguration", "IX_GoPipelineName_PackageConfigurationId");
            DropIndex("Go.PackageGoPipelineConfiguration", new[] { "ExternalId" });
            DropIndex("Go.PackageConfiguration", new[] { "GoServerConfigurationId" });
            DropIndex("Go.PackageConfiguration", new[] { "PackageId" });
            DropIndex("Go.GoServerConfiguration", new[] { "GlobalConfigurationId" });
            DropIndex("Go.GoServerConfiguration", new[] { "Name" });
            DropTable("Go.PackageGoPipelineConfiguration");
            DropTable("Go.PackageConfiguration");
            DropTable("Go.GoServerConfiguration");
            DropTable("Go.GlobalConfiguration");
        }
    }
}
