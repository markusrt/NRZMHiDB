namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Patient : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Patients",
                c => new
                    {
                        PatientId = c.Int(nullable: false, identity: true),
                        Initials = c.String(nullable: false),
                        BirthDate = c.DateTime(),
                        PostalCode = c.String(),
                        Gender = c.Int(nullable: false),
                        City = c.String(),
                        County = c.String(),
                        State = c.Int(nullable: false),
                        ClinicalInformation = c.String(),
                        HibVaccination = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PatientId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Patients");
        }
    }
}
