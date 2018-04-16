namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MeningoPatient_3_RiskFactors : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MeningoPatients", "RiskFactors", c => c.Int(nullable: false));
            AddColumn("dbo.MeningoPatients", "OtherRiskFactor", c => c.String());
            DropColumn("dbo.MeningoPatients", "Residua");
        }
        
        public override void Down()
        {
            AddColumn("dbo.MeningoPatients", "Residua", c => c.String());
            DropColumn("dbo.MeningoPatients", "OtherRiskFactor");
            DropColumn("dbo.MeningoPatients", "RiskFactors");
        }
    }
}
