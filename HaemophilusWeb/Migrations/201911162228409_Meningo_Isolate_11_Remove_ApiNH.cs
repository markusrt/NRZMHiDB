namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Meningo_Isolate_11_Remove_ApiNH : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.MeningoIsolates", "ApiNh");
            DropColumn("dbo.MeningoIsolates", "ApiNhBestMatch");
            DropColumn("dbo.MeningoIsolates", "ApiNhMatchInPercent");
        }
        
        public override void Down()
        {
            AddColumn("dbo.MeningoIsolates", "ApiNhMatchInPercent", c => c.Double());
            AddColumn("dbo.MeningoIsolates", "ApiNhBestMatch", c => c.String());
            AddColumn("dbo.MeningoIsolates", "ApiNh", c => c.Int(nullable: false));
        }
    }
}
