namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Meningo_Isolate_12_Add_SIAA_CNL_CRTA : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MeningoIsolates", "SiaAGene", c => c.Int(nullable: false));
            AddColumn("dbo.MeningoIsolates", "CapsularTransferGene", c => c.Int(nullable: false));
            AddColumn("dbo.MeningoIsolates", "CapsuleNullLocus", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MeningoIsolates", "CapsuleNullLocus");
            DropColumn("dbo.MeningoIsolates", "CapsularTransferGene");
            DropColumn("dbo.MeningoIsolates", "SiaAGene");
        }
    }
}
