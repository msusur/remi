namespace ReMi.Plugin.ZenDesk.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class InitialiseDatabase : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "ZenDesk.GlobalConfiguration",
                c => new
                    {
                        GlobalConfigurationId = c.Int(nullable: false, identity: true),
                        ZenDeskUrl = c.String(),
                        ZenDeskUser = c.String(),
                        ZenDeskPassword = c.String(),
                    })
                .PrimaryKey(t => t.GlobalConfigurationId);
            
        }
        
        public override void Down()
        {
            DropTable("ZenDesk.GlobalConfiguration");
        }
    }
}
