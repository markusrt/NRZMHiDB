namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Isolate_12_ReportStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Isolates", "ReportStatus", c => c.Int(nullable: false));
            Sql("UPDATE dbo.Isolates SET ReportStatus = CASE WHEN ReportDate IS NOT NULL THEN 2 ELSE 0 END");
        }
        
        public override void Down()
        {
            DropColumn("dbo.Isolates", "ReportStatus");
        }
    }
}
