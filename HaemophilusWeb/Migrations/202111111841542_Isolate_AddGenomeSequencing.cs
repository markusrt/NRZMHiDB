namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Isolate_AddGenomeSequencing : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Isolates", "GenomeSequencing", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Isolates", "GenomeSequencing");
        }
    }
}
