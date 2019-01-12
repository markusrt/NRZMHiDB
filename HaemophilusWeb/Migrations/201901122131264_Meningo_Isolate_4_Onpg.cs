namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Meningo_Isolate_4_Onpg : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MeningoIsolates", "Onpg", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MeningoIsolates", "Onpg");
        }
    }
}
