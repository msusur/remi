namespace ReMi.Plugin.EmailMock.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class InitializeDatabase : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "EmailMock.GlobalConfiguration",
                c => new
                    {
                        GlobalConfigurationId = c.Int(nullable: false, identity: true),
                        RedirectToEmail = c.String(),
                    })
                .PrimaryKey(t => t.GlobalConfigurationId);
            
        }
        
        public override void Down()
        {
            DropTable("EmailMock.GlobalConfiguration");
        }
    }
}
