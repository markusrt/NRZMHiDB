namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Sending : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Sendings",
                c => new
                    {
                        SendingId = c.Int(nullable: false, identity: true),
                        SenderId = c.Int(nullable: false),
                        PatientId = c.Int(nullable: false),
                        SamplingDate = c.DateTime(nullable: false),
                        ReceivingDate = c.DateTime(nullable: false),
                        Material = c.Int(nullable: false),
                        OtherMaterial = c.String(),
                        Invasive = c.Int(),
                        SenderLaboratoryNumber = c.String(),
                        SenderConclusion = c.String(),
                        Evaluation = c.Int(nullable: false),
                        Remark = c.String(),
                        ReportDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.SendingId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Sendings");
        }
    }
}
