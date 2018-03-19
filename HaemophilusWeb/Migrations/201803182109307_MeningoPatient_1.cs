namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MeningoPatient_1 : DbMigration
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
                        UnderlyingDisease = c.String(),
                        Residua = c.String(),
                        Epidemiology = c.Int(nullable: false),
                        Initials = c.String(),
                        BirthDate = c.DateTime(),
                        PostalCode = c.String(),
                        Gender = c.Int(),
                        City = c.String(),
                        County = c.String(),
                        State = c.Int(nullable: false),
                        Therapy = c.Int(nullable: false),
                        TherapyDetails = c.String(),
                    })
                .PrimaryKey(t => t.PatientId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.MeningoPatients");
        }
    }
}
