namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MeningoPatientAndSending : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MeningoPatients",
                c => new
                    {
                        PatientId = c.Int(nullable: false, identity: true),
                        ClinicalInformation = c.Int(nullable: false),
                        OtherClinicalInformation = c.String(),
                        RiskFactors = c.Int(nullable: false),
                        OtherRiskFactor = c.String(),
                        Initials = c.String(),
                        BirthDate = c.DateTime(),
                        PostalCode = c.String(),
                        Gender = c.Int(),
                        City = c.String(),
                        County = c.String(),
                        State = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PatientId);
            
            CreateTable(
                "dbo.MeningoSendings",
                c => new
                    {
                        MeningoSendingId = c.Int(nullable: false, identity: true),
                        MeningoPatientId = c.Int(nullable: false),
                        SamplingLocation = c.Int(nullable: false),
                        Material = c.Int(nullable: false),
                        SerogroupSender = c.String(),
                        SenderId = c.Int(nullable: false),
                        SamplingDate = c.DateTime(),
                        ReceivingDate = c.DateTime(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                        SenderLaboratoryNumber = c.String(),
                        Invasive = c.Int(),
                        OtherSamplingLocation = c.String(),
                        Remark = c.String(),
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
            DropTable("dbo.MeningoPatients");
        }
    }
}
