namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Meningo_Patient_2_RemovedFields : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.MeningoPatients", "UnderlyingDisease");
            DropColumn("dbo.MeningoPatients", "OtherUnderlyingDisease");
            DropColumn("dbo.MeningoPatients", "Epidemiology");
            DropColumn("dbo.MeningoPatients", "Therapy");
            DropColumn("dbo.MeningoPatients", "TherapyDetails");
        }
        
        public override void Down()
        {
            AddColumn("dbo.MeningoPatients", "TherapyDetails", c => c.String());
            AddColumn("dbo.MeningoPatients", "Therapy", c => c.Int(nullable: false));
            AddColumn("dbo.MeningoPatients", "Epidemiology", c => c.Int(nullable: false));
            AddColumn("dbo.MeningoPatients", "OtherUnderlyingDisease", c => c.String());
            AddColumn("dbo.MeningoPatients", "UnderlyingDisease", c => c.Int(nullable: false));
        }
    }
}
