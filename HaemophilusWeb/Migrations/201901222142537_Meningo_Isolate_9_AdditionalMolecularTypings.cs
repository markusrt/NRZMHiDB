namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Meningo_Isolate_9_AdditionalMolecularTypings : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MeningoIsolates", "PorAPcr", c => c.Int(nullable: false));
            AddColumn("dbo.MeningoIsolates", "FetAPcr", c => c.Int(nullable: false));
            AddColumn("dbo.MeningoIsolates", "CsbPcr", c => c.Int(nullable: false));
            AddColumn("dbo.MeningoIsolates", "CscPcr", c => c.Int(nullable: false));
            AddColumn("dbo.MeningoIsolates", "CswyPcr", c => c.Int(nullable: false));
            AddColumn("dbo.MeningoIsolates", "CswyAllele", c => c.Int(nullable: false));
            AddColumn("dbo.MeningoIsolates", "NhsRealTimePcr", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MeningoIsolates", "NhsRealTimePcr");
            DropColumn("dbo.MeningoIsolates", "CswyAllele");
            DropColumn("dbo.MeningoIsolates", "CswyPcr");
            DropColumn("dbo.MeningoIsolates", "CscPcr");
            DropColumn("dbo.MeningoIsolates", "CsbPcr");
            DropColumn("dbo.MeningoIsolates", "FetAPcr");
            DropColumn("dbo.MeningoIsolates", "PorAPcr");
        }
    }
}
