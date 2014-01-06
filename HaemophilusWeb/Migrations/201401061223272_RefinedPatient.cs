namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RefinedPatient : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Patients", "OtherClinicalInformation", c => c.String());
            AddColumn("dbo.Patients", "HibVaccinationDate", c => c.DateTime());
            AlterColumn("dbo.Patients", "ClinicalInformation", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Patients", "ClinicalInformation", c => c.String());
            DropColumn("dbo.Patients", "HibVaccinationDate");
            DropColumn("dbo.Patients", "OtherClinicalInformation");
        }
    }
}
