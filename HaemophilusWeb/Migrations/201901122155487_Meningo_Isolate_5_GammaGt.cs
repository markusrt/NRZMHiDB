namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Meningo_Isolate_5_GammaGt : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MeningoIsolates", "GammaGt", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.MeningoIsolates", "GammaGt");
        }
    }
}
