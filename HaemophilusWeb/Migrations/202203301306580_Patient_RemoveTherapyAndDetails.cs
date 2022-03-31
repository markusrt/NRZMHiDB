namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Patient_RemoveTherapyAndDetails : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Patients", "Therapy");
            DropColumn("dbo.Patients", "TherapyDetails");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Patients", "TherapyDetails", c => c.String());
            AddColumn("dbo.Patients", "Therapy", c => c.Int(nullable: false));
        }
    }
}
