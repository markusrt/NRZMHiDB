namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameMaldiTofToVitek : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.Isolates", "MaldiTof", "MaldiTofVitek");
            RenameColumn("dbo.Isolates", "MaldiTofBestMatch", "MaldiTofVitekBestMatch");
            RenameColumn("dbo.Isolates", "MaldiTofMatchConfidence", "MaldiTofVitekMatchConfidence");
            RenameColumn("dbo.MeningoIsolates", "MaldiTof", "MaldiTofVitek");
            RenameColumn("dbo.MeningoIsolates", "MaldiTofBestMatch", "MaldiTofVitekBestMatch");
            RenameColumn("dbo.MeningoIsolates", "MaldiTofMatchConfidence", "MaldiTofVitekMatchConfidence");
        }
        
        public override void Down()
        {
            RenameColumn("dbo.Isolates", "MaldiTofVitek", "MaldiTof");
            RenameColumn("dbo.Isolates", "MaldiTofVitekBestMatch", "MaldiTofBestMatch");
            RenameColumn("dbo.Isolates", "MaldiTofVitekMatchConfidence", "MaldiTofMatchConfidence");
            RenameColumn("dbo.MeningoIsolates", "MaldiTofVitek", "MaldiTof");
            RenameColumn("dbo.MeningoIsolates", "MaldiTofVitekBestMatch", "MaldiTofBestMatch");
            RenameColumn("dbo.MeningoIsolates", "MaldiTofVitekMatchConfidence", "MaldiTofMatchConfidence");
        }
    }
}
