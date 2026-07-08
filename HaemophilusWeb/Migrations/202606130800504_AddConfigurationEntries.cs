namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddConfigurationEntries : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ConfigurationEntries",
                c => new
                    {
                        ConfigurationEntryId = c.Int(nullable: false, identity: true),
                        Key = c.String(nullable: false, maxLength: 400),
                        Value = c.String(),
                    })
                .PrimaryKey(t => t.ConfigurationEntryId)
                .Index(t => t.Key, unique: true);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.ConfigurationEntries", new[] { "Key" });
            DropTable("dbo.ConfigurationEntries");
        }
    }
}
