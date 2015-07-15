namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Sender_Delete : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Senders", "Deleted", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Senders", "Deleted");
        }
    }
}
