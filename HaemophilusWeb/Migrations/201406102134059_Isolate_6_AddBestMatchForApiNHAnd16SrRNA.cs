namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Isolate_6_AddBestMatchForApiNHAnd16SrRNA : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Isolates", "RibosomalRna16SBestMatch", c => c.String());
            AddColumn("dbo.Isolates", "ApiNhBestMatch", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Isolates", "ApiNhBestMatch");
            DropColumn("dbo.Isolates", "RibosomalRna16SBestMatch");
        }
    }
}
