namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Meningo_Isolate_6_SerogroupPcr : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MeningoIsolates", "SerogroupPcr", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MeningoIsolates", "SerogroupPcr");
        }
    }
}
