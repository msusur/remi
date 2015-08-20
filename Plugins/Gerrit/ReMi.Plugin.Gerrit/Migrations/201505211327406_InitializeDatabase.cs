namespace ReMi.Plugin.Gerrit.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class InitializeDatabase : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Gerrit.GlobalConfiguration",
                c => new
                    {
                        GlobalConfigurationId = c.Int(nullable: false, identity: true),
                        Host = c.String(),
                        Port = c.Int(nullable: false),
                        User = c.String(),
                        PrivateKey = c.String(),
                    })
                .PrimaryKey(t => t.GlobalConfigurationId);
            
            CreateTable(
                "Gerrit.PackageConfiguration",
                c => new
                    {
                        PackageConfigurationId = c.Int(nullable: false, identity: true),
                        PackageId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.PackageConfigurationId)
                .Index(t => t.PackageId, unique: true);
            
            CreateTable(
                "Gerrit.Repositories",
                c => new
                    {
                        RepositoryId = c.Int(nullable: false, identity: true),
                        ExternalId = c.Guid(nullable: false),
                        Name = c.String(maxLength: 256),
                        DefaultFrom = c.String(),
                        DefaultTo = c.String(),
                        StartFromLatest = c.Boolean(nullable: false),
                        IsIncludedByDefault = c.Boolean(nullable: false),
                        IsDisabled = c.Boolean(nullable: false),
                        PackageConfigurationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.RepositoryId)
                .ForeignKey("Gerrit.PackageConfiguration", t => t.PackageConfigurationId, cascadeDelete: true)
                .Index(t => t.ExternalId, unique: true)
                .Index(t => new { t.Name, t.PackageConfigurationId }, unique: true);
            
        }
        
        public override void Down()
        {
            DropForeignKey("Gerrit.Repositories", "PackageConfigurationId", "Gerrit.PackageConfiguration");
            DropIndex("Gerrit.Repositories", new[] { "Name", "PackageConfigurationId" });
            DropIndex("Gerrit.Repositories", new[] { "ExternalId" });
            DropIndex("Gerrit.PackageConfiguration", new[] { "PackageId" });
            DropTable("Gerrit.Repositories");
            DropTable("Gerrit.PackageConfiguration");
            DropTable("Gerrit.GlobalConfiguration");
        }
    }
}
