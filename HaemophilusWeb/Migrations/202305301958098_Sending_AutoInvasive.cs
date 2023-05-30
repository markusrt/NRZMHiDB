namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Sending_AutoInvasive : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Sendings", "Invasive");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Sendings", "Invasive", c => c.Int());
        }
    }
}
