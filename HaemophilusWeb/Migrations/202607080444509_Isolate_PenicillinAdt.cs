namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Isolate_PenicillinAdt : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Isolates", "PenicillinAdt", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Isolates", "PenicillinAdt");
        }
    }
}
