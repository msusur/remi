namespace ReMi.Plugin.Ldap.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class InitializeDatabase : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Ldap.GlobalConfiguration",
                c => new
                    {
                        GlobalConfigurationId = c.Int(nullable: false, identity: true),
                        UserName = c.String(),
                        Password = c.String(),
                        LdapPath = c.String(),
                        SearchCriteria = c.String(),
                    })
                .PrimaryKey(t => t.GlobalConfigurationId);
            
        }
        
        public override void Down()
        {
            DropTable("Ldap.GlobalConfiguration");
        }
    }
}
