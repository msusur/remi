namespace ReMi.Plugin.Jenkins.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class InitializeDatabase : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Jenkins.GlobalConfiguration",
                c => new
                    {
                        GlobalConfigurationId = c.Int(nullable: false, identity: true),
                        JenkinsUser = c.String(),
                        JenkinsPassword = c.String(),
                    })
                .PrimaryKey(t => t.GlobalConfigurationId);
            
            CreateTable(
                "Jenkins.JenkinsServerConfiguration",
                c => new
                    {
                        JenkinsServerConfigurationId = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 256),
                        Value = c.String(),
                        GlobalConfigurationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.JenkinsServerConfigurationId)
                .ForeignKey("Jenkins.GlobalConfiguration", t => t.GlobalConfigurationId, cascadeDelete: true)
                .Index(t => t.Name, unique: true)
                .Index(t => t.GlobalConfigurationId);
            
            CreateTable(
                "Jenkins.PackageConfiguration",
                c => new
                    {
                        PackageConfigurationId = c.Int(nullable: false, identity: true),
                        PackageId = c.Guid(nullable: false),
                        JenkinsServerConfigurationId = c.Int(),
                    })
                .PrimaryKey(t => t.PackageConfigurationId)
                .ForeignKey("Jenkins.JenkinsServerConfiguration", t => t.JenkinsServerConfigurationId)
                .Index(t => t.PackageId, unique: true)
                .Index(t => t.JenkinsServerConfigurationId);
            
            CreateTable(
                "Jenkins.PackageJenkinsJobConfiguration",
                c => new
                    {
                        PackageJenkinsJobConfigurationId = c.Int(nullable: false, identity: true),
                        ExternalId = c.Guid(nullable: false),
                        Name = c.String(maxLength: 256),
                        IsIncludedByDefault = c.Boolean(nullable: false),
                        IsDisabled = c.Boolean(nullable: false),
                        PackageConfigurationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PackageJenkinsJobConfigurationId)
                .ForeignKey("Jenkins.PackageConfiguration", t => t.PackageConfigurationId, cascadeDelete: true)
                .Index(t => t.ExternalId, unique: true)
                .Index(t => new { t.Name, t.PackageConfigurationId }, unique: true, name: "IX_JenkinsJobName_PackageConfigurationId");
            
        }
        
        public override void Down()
        {
            DropForeignKey("Jenkins.PackageJenkinsJobConfiguration", "PackageConfigurationId", "Jenkins.PackageConfiguration");
            DropForeignKey("Jenkins.PackageConfiguration", "JenkinsServerConfigurationId", "Jenkins.JenkinsServerConfiguration");
            DropForeignKey("Jenkins.JenkinsServerConfiguration", "GlobalConfigurationId", "Jenkins.GlobalConfiguration");
            DropIndex("Jenkins.PackageJenkinsJobConfiguration", "IX_JenkinsJobName_PackageConfigurationId");
            DropIndex("Jenkins.PackageJenkinsJobConfiguration", new[] { "ExternalId" });
            DropIndex("Jenkins.PackageConfiguration", new[] { "JenkinsServerConfigurationId" });
            DropIndex("Jenkins.PackageConfiguration", new[] { "PackageId" });
            DropIndex("Jenkins.JenkinsServerConfiguration", new[] { "GlobalConfigurationId" });
            DropIndex("Jenkins.JenkinsServerConfiguration", new[] { "Name" });
            DropTable("Jenkins.PackageJenkinsJobConfiguration");
            DropTable("Jenkins.PackageConfiguration");
            DropTable("Jenkins.JenkinsServerConfiguration");
            DropTable("Jenkins.GlobalConfiguration");
        }
    }
}
