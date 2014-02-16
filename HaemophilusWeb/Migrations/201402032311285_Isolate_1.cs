namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Isolate_1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Isolates",
                c => new
                    {
                        SendingId = c.Int(nullable: false),
                        IsolateId = c.Int(nullable: false, identity: true),
                        YearlySequentialIsolateNumber = c.Int(nullable: false),
                        Year = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.SendingId)
                .ForeignKey("dbo.Sendings", t => t.SendingId)
                .Index(t => t.SendingId);

            CreateIndex("dbo.Isolates", new[] {"YearlySequentialIsolateNumber", "Year"}, true, "IX_LaboratoryNumber");

        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Isolates", "SendingId", "dbo.Sendings");
            DropIndex("dbo.Isolates", new[] { "SendingId" });
            DropTable("dbo.Isolates");
        }
    }
}
