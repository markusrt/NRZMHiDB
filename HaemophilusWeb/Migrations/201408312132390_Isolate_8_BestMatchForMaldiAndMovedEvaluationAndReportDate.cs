namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Isolate_8_BestMatchForMaldiAndMovedEvaluationAndReportDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Isolates", "MaldiTofBestMatch", c => c.String());
            AddColumn("dbo.Isolates", "Evaluation", c => c.Int(nullable: false));
            AddColumn("dbo.Isolates", "ReportDate", c => c.DateTime());
            DropColumn("dbo.Sendings", "Evaluation");
            DropColumn("dbo.Sendings", "ReportDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Sendings", "ReportDate", c => c.DateTime());
            AddColumn("dbo.Sendings", "Evaluation", c => c.Int(nullable: false));
            DropColumn("dbo.Isolates", "ReportDate");
            DropColumn("dbo.Isolates", "Evaluation");
            DropColumn("dbo.Isolates", "MaldiTofBestMatch");
        }
    }
}
