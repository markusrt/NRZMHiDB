namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RefinedSender1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Senders", "StreetWithNumber", c => c.String());
            AddColumn("dbo.Senders", "PostalCode", c => c.String());
            AddColumn("dbo.Senders", "City", c => c.String());
            DropColumn("dbo.Senders", "ContactPerson");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Senders", "ContactPerson", c => c.String(nullable: false));
            DropColumn("dbo.Senders", "City");
            DropColumn("dbo.Senders", "PostalCode");
            DropColumn("dbo.Senders", "StreetWithNumber");
        }
    }
}
