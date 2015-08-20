namespace ReMi.Plugin.Jira.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitDb : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Jira.GlobalConfiguration",
                c => new
                    {
                        GlobalConfigurationId = c.Int(nullable: false, identity: true),
                        JiraUrl = c.String(),
                        JiraBrowseUrl = c.String(),
                        JiraIssuesMaxCount = c.Int(nullable: false),
                        JiraUser = c.String(),
                        JiraPassword = c.String(),
                    })
                .PrimaryKey(t => t.GlobalConfigurationId);
            
            CreateTable(
                "Jira.PackageConfiguration",
                c => new
                    {
                        PackageConfigurationId = c.Int(nullable: false, identity: true),
                        PackageId = c.Guid(nullable: false),
                        UpdateTicketMode = c.Int(nullable: false),
                        Label = c.String(),
                    })
                .PrimaryKey(t => t.PackageConfigurationId)
                .Index(t => t.PackageId, unique: true);
            
            CreateTable(
                "Jira.PackageDefectJqlFilters",
                c => new
                    {
                        PackageDefectJqlFilterId = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 256),
                        Value = c.String(),
                        PackageConfigurationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PackageDefectJqlFilterId)
                .ForeignKey("Jira.PackageConfiguration", t => t.PackageConfigurationId, cascadeDelete: true)
                .Index(t => new { t.Name, t.PackageConfigurationId }, unique: true, name: "IX_DefectJqlName_PackageConfigurationId");
            
            CreateTable(
                "Jira.PackageJqlFilters",
                c => new
                    {
                        PackageJqlFilterId = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 256),
                        Value = c.String(),
                        PackageConfigurationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PackageJqlFilterId)
                .ForeignKey("Jira.PackageConfiguration", t => t.PackageConfigurationId, cascadeDelete: true)
                .Index(t => new { t.Name, t.PackageConfigurationId }, unique: true, name: "IX_JqlFiltersName_PackageConfigurationId");
            
            CreateTable(
                "Jira.UpdateTicketModes",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true);
            
        }
        
        public override void Down()
        {
            DropForeignKey("Jira.PackageJqlFilters", "PackageConfigurationId", "Jira.PackageConfiguration");
            DropForeignKey("Jira.PackageDefectJqlFilters", "PackageConfigurationId", "Jira.PackageConfiguration");
            DropIndex("Jira.UpdateTicketModes", new[] { "Name" });
            DropIndex("Jira.PackageJqlFilters", "IX_JqlFiltersName_PackageConfigurationId");
            DropIndex("Jira.PackageDefectJqlFilters", "IX_DefectJqlName_PackageConfigurationId");
            DropIndex("Jira.PackageConfiguration", new[] { "PackageId" });
            DropTable("Jira.UpdateTicketModes");
            DropTable("Jira.PackageJqlFilters");
            DropTable("Jira.PackageDefectJqlFilters");
            DropTable("Jira.PackageConfiguration");
            DropTable("Jira.GlobalConfiguration");
        }
    }
}
