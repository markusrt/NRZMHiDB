namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Sending_SerotypeSender : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sendings", "SerotypeSender", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sendings", "SerotypeSender");
        }
    }
}
