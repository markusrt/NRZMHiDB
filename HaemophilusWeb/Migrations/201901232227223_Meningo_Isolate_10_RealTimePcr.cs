namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Meningo_Isolate_10_RealTimePcr : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MeningoIsolates", "RealTimePcr", c => c.Int(nullable: false));
            AddColumn("dbo.MeningoIsolates", "RealTimePcrResult", c => c.Int(nullable: false));
            DropColumn("dbo.MeningoIsolates", "NhsRealTimePcr");
        }
        
        public override void Down()
        {
            AddColumn("dbo.MeningoIsolates", "NhsRealTimePcr", c => c.Int(nullable: false));
            DropColumn("dbo.MeningoIsolates", "RealTimePcrResult");
            DropColumn("dbo.MeningoIsolates", "RealTimePcr");
        }
    }
}
