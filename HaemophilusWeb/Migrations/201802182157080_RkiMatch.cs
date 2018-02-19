namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RkiMatch : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RkiMatchRecords",
                c => new
                    {
                        SendingId = c.Int(nullable: false),
                        RkiReferenceId = c.Int(nullable: false),
                        RkiReferenceNumber = c.String(),
                        RkiStatus = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.SendingId)
                .ForeignKey("dbo.Sendings", t => t.SendingId)
                .Index(t => t.SendingId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RkiMatchRecords", "SendingId", "dbo.Sendings");
            DropIndex("dbo.RkiMatchRecords", new[] { "SendingId" });
            DropTable("dbo.RkiMatchRecords");
        }
    }
}
