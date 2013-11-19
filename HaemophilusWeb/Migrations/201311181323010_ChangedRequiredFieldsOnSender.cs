namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangedRequiredFieldsOnSender : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Senders", "ContactPerson", c => c.String(nullable: false));
            AlterColumn("dbo.Senders", "Phone1", c => c.String(nullable: false));
            AlterColumn("dbo.Senders", "Email", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Senders", "Email", c => c.String(nullable: false));
            AlterColumn("dbo.Senders", "Phone1", c => c.String());
            AlterColumn("dbo.Senders", "ContactPerson", c => c.String());
        }
    }
}
