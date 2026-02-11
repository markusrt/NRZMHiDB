namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsolateCommon_RealTimePcrDevice : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MeningoIsolates", "RealTimePcrDevice", c => c.Int(nullable: false, defaultValue: 0));
            AddColumn("dbo.Isolates", "RealTimePcrDevice", c => c.Int(nullable: false, defaultValue: 0));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Isolates", "RealTimePcrDevice");
            DropColumn("dbo.MeningoIsolates", "RealTimePcrDevice");
        }
    }
}
