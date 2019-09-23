namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Patient_Country : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Patients", "Country", c => c.String());
            AddColumn("dbo.MeningoPatients", "Country", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.MeningoPatients", "Country");
            DropColumn("dbo.Patients", "Country");
        }
    }
}
