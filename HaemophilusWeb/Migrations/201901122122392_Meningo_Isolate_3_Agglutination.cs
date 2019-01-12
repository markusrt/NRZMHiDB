namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Meningo_Isolate_3_Agglutination : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MeningoIsolates", "Agglutination", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MeningoIsolates", "Agglutination");
        }
    }
}
