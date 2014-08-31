namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RefinedPatient2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Patients", "Therapy", c => c.Int(nullable: false));
            AddColumn("dbo.Patients", "TherapyDetails", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Patients", "TherapyDetails");
            DropColumn("dbo.Patients", "Therapy");
        }
    }
}
