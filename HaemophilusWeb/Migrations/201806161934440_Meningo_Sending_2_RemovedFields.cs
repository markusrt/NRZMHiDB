namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Meningo_Sending_2_RemovedFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MeningoSendings", "SerogroupSender", c => c.String());
            AddColumn("dbo.MeningoSendings", "Remark", c => c.String());
            DropColumn("dbo.MeningoSendings", "SenderConclusion");
        }
        
        public override void Down()
        {
            AddColumn("dbo.MeningoSendings", "SenderConclusion", c => c.String());
            DropColumn("dbo.MeningoSendings", "Remark");
            DropColumn("dbo.MeningoSendings", "SerogroupSender");
        }
    }
}
