namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class Meningo_Isolate_7_Growth : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MeningoIsolates", "GrowthOnBloodAgar", c => c.Int(nullable: false));
            AddColumn("dbo.MeningoIsolates", "GrowthOnMartinLewisAgar", c => c.Int(nullable: false));
            DropColumn("dbo.MeningoIsolates", "Growth");
            DropColumn("dbo.MeningoIsolates", "TypeOfGrowth");
        }

        public override void Down()
        {
            AddColumn("dbo.MeningoIsolates", "TypeOfGrowth", c => c.Int(nullable: false));
            AddColumn("dbo.MeningoIsolates", "Growth", c => c.Int(nullable: false));
            DropColumn("dbo.MeningoIsolates", "GrowthOnMartinLewisAgar");
            DropColumn("dbo.MeningoIsolates", "GrowthOnBloodAgar");
        }
    }
}
