namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Isolate_MaldiTofBiotyper : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Isolates", "MaldiTofBiotyper", c => c.Int(nullable: false));
            AddColumn("dbo.Isolates", "MaldiTofBiotyperBestMatch", c => c.String());
            AddColumn("dbo.Isolates", "MaldiTofBiotyperMatchConfidence", c => c.Double());
            AddColumn("dbo.MeningoIsolates", "MaldiTofBiotyper", c => c.Int(nullable: false));
            AddColumn("dbo.MeningoIsolates", "MaldiTofBiotyperBestMatch", c => c.String());
            AddColumn("dbo.MeningoIsolates", "MaldiTofBiotyperMatchConfidence", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.MeningoIsolates", "MaldiTofBiotyperMatchConfidence");
            DropColumn("dbo.MeningoIsolates", "MaldiTofBiotyperBestMatch");
            DropColumn("dbo.MeningoIsolates", "MaldiTofBiotyper");
            DropColumn("dbo.Isolates", "MaldiTofBiotyperMatchConfidence");
            DropColumn("dbo.Isolates", "MaldiTofBiotyperBestMatch");
            DropColumn("dbo.Isolates", "MaldiTofBiotyper");
        }
    }
}
