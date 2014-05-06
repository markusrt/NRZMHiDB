namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Isolate_5_AddMatchesAndMaldiTof : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Isolates", "RibosomalRna16SMatchInPercent", c => c.Double());
            AddColumn("dbo.Isolates", "ApiNhMatchInPercent", c => c.Double());
            AddColumn("dbo.Isolates", "MaldiTof", c => c.Int(nullable: false));
            AddColumn("dbo.Isolates", "MaldiTofMatchConfidence", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Isolates", "MaldiTofMatchConfidence");
            DropColumn("dbo.Isolates", "MaldiTof");
            DropColumn("dbo.Isolates", "ApiNhMatchInPercent");
            DropColumn("dbo.Isolates", "RibosomalRna16SMatchInPercent");
        }
    }
}
