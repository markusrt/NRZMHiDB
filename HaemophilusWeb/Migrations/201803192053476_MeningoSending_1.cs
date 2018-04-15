namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MeningoSending_1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MeningoSendings",
                c => new
                    {
                        MeningoSendingId = c.Int(nullable: false, identity: true),
                        MeningoPatientId = c.Int(nullable: false),
                        SamplingLocation = c.Int(nullable: false),
                        Material = c.Int(nullable: false),
                        OtherSamplingLocation = c.String(),
                        SenderId = c.Int(nullable: false),
                        SamplingDate = c.DateTime(),
                        ReceivingDate = c.DateTime(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                        SenderLaboratoryNumber = c.String(),
                        Invasive = c.Int(),
                    })
                .PrimaryKey(t => t.MeningoSendingId)
                .ForeignKey("dbo.MeningoPatients", t => t.MeningoPatientId, cascadeDelete: true)
                .Index(t => t.MeningoPatientId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MeningoSendings", "MeningoPatientId", "dbo.MeningoPatients");
            DropIndex("dbo.MeningoSendings", new[] { "MeningoPatientId" });
            DropTable("dbo.MeningoSendings");
        }
    }
}
