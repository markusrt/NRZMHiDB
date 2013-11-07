namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RefinedSender : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Senders", "Department", c => c.String());
            AddColumn("dbo.Senders", "ContactPerson", c => c.String());
            AddColumn("dbo.Senders", "Phone1", c => c.String());
            AddColumn("dbo.Senders", "Phone2", c => c.String());
            AddColumn("dbo.Senders", "Fax", c => c.String());
            AddColumn("dbo.Senders", "Email", c => c.String(nullable: false));
            AddColumn("dbo.Senders", "Remark", c => c.String());
            DropColumn("dbo.Senders", "Type");
            DropColumn("dbo.Senders", "PostalCode");
            DropColumn("dbo.Senders", "City");
            DropColumn("dbo.Senders", "Street");
            DropColumn("dbo.Senders", "ContactInfo");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Senders", "ContactInfo", c => c.String());
            AddColumn("dbo.Senders", "Street", c => c.String(nullable: false));
            AddColumn("dbo.Senders", "City", c => c.String(nullable: false));
            AddColumn("dbo.Senders", "PostalCode", c => c.String(nullable: false));
            AddColumn("dbo.Senders", "Type", c => c.String());
            DropColumn("dbo.Senders", "Remark");
            DropColumn("dbo.Senders", "Email");
            DropColumn("dbo.Senders", "Fax");
            DropColumn("dbo.Senders", "Phone2");
            DropColumn("dbo.Senders", "Phone1");
            DropColumn("dbo.Senders", "ContactPerson");
            DropColumn("dbo.Senders", "Department");
        }
    }
}
