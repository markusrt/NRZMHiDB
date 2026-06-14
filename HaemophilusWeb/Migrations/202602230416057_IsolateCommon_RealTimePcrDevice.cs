namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsolateCommon_RealTimePcrDevice : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Isolates", "RealTimePcrDevice", c => c.Int(nullable: false, defaultValue: 2));
            AddColumn("dbo.MeningoIsolates", "RealTimePcrDevice", c => c.Int(nullable: false, defaultValue: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MeningoIsolates", "RealTimePcrDevice");
            DropColumn("dbo.Isolates", "RealTimePcrDevice");
        }
    }
}
