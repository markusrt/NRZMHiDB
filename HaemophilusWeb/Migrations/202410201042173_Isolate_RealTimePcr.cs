namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Isolate_RealTimePcr : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Isolates", "RealTimePcr", c => c.Int(nullable: false));
            AddColumn("dbo.Isolates", "RealTimePcrResult", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Isolates", "RealTimePcrResult");
            DropColumn("dbo.Isolates", "RealTimePcr");
        }
    }
}
