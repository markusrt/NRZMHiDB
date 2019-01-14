namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class MeningoSending_2_InvasiveAndNonInvasiveSamplingLocation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MeningoSendings", "OtherInvasiveSamplingLocation", c => c.String());
            AddColumn("dbo.MeningoSendings", "OtherNonInvasiveSamplingLocation", c => c.String());
            DropColumn("dbo.MeningoSendings", "Invasive");
            DropColumn("dbo.MeningoSendings", "OtherSamplingLocation");
        }

        public override void Down()
        {
            AddColumn("dbo.MeningoSendings", "OtherSamplingLocation", c => c.String());
            AddColumn("dbo.MeningoSendings", "Invasive", c => c.Int());
            DropColumn("dbo.MeningoSendings", "OtherNonInvasiveSamplingLocation");
            DropColumn("dbo.MeningoSendings", "OtherInvasiveSamplingLocation");
        }
    }
}
