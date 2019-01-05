namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Meningo_Isolate_1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MeningoIsolates",
                c => new
                    {
                        MeningoSendingId = c.Int(nullable: false),
                        MeningoIsolateId = c.Int(nullable: false, identity: true),
                        StemNumber = c.Int(),
                        Oxidase = c.Int(nullable: false),
                        RibosomalRna16S = c.Int(nullable: false),
                        RibosomalRna16SBestMatch = c.String(),
                        RibosomalRna16SMatchInPercent = c.Double(),
                        ApiNh = c.Int(nullable: false),
                        ApiNhBestMatch = c.String(),
                        ApiNhMatchInPercent = c.Double(),
                        MaldiTof = c.Int(nullable: false),
                        MaldiTofBestMatch = c.String(),
                        MaldiTofMatchConfidence = c.Double(),
                        ReportDate = c.DateTime(),
                        ReportStatus = c.Int(nullable: false),
                        Remark = c.String(),
                        Growth = c.Int(nullable: false),
                        YearlySequentialIsolateNumber = c.Int(nullable: false),
                        Year = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MeningoSendingId)
                .ForeignKey("dbo.MeningoSendings", t => t.MeningoSendingId)
                .Index(t => t.MeningoSendingId);
            
            AddColumn("dbo.EpsilometerTests", "MeningoIsolate_MeningoSendingId", c => c.Int());
            CreateIndex("dbo.EpsilometerTests", "MeningoIsolate_MeningoSendingId");
            AddForeignKey("dbo.EpsilometerTests", "MeningoIsolate_MeningoSendingId", "dbo.MeningoIsolates", "MeningoSendingId");
            CreateIndex("dbo.MeningoIsolates", new[] { "StemNumber" }, true, "IX_StemNumber");
            CreateIndex("dbo.MeningoIsolates", new[] { "YearlySequentialIsolateNumber", "Year" }, true, "IX_LaboratoryNumber");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Isolates", "IX_LaboratoryNumber");
            DropIndex("dbo.MeningoIsolates", "IX_StemNumber");
            DropForeignKey("dbo.MeningoIsolates", "MeningoSendingId", "dbo.MeningoSendings");
            DropForeignKey("dbo.EpsilometerTests", "MeningoIsolate_MeningoSendingId", "dbo.MeningoIsolates");
            DropIndex("dbo.MeningoIsolates", new[] { "MeningoSendingId" });
            DropIndex("dbo.EpsilometerTests", new[] { "MeningoIsolate_MeningoSendingId" });
            DropColumn("dbo.EpsilometerTests", "MeningoIsolate_MeningoSendingId");
            DropTable("dbo.MeningoIsolates");
        }
    }
}
