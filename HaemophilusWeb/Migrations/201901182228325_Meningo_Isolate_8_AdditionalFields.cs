namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class Meningo_Isolate_8_AdditionalFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MeningoIsolates", "PorAVr1", c => c.String());
            AddColumn("dbo.MeningoIsolates", "PorAVr2", c => c.String());
            AddColumn("dbo.MeningoIsolates", "FetAVr", c => c.String());
            AddColumn("dbo.MeningoIsolates", "RplF", c => c.String());
            AlterColumn("dbo.MeningoIsolates", "GammaGt", c => c.Int(nullable: false));
        }

        public override void Down()
        {
            AlterColumn("dbo.MeningoIsolates", "GammaGt", c => c.Double());
            DropColumn("dbo.MeningoIsolates", "RplF");
            DropColumn("dbo.MeningoIsolates", "FetAVr");
            DropColumn("dbo.MeningoIsolates", "PorAVr2");
            DropColumn("dbo.MeningoIsolates", "PorAVr1");
        }
    }
}
