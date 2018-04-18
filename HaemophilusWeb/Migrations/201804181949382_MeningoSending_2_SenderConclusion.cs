namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MeningoSending_2_SenderConclusion : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MeningoSendings", "SenderConclusion", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.MeningoSendings", "SenderConclusion");
        }
    }
}
