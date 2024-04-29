namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SendingBase_DemisId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sendings", "DemisId", c => c.Guid());
            AddColumn("dbo.MeningoSendings", "DemisId", c => c.Guid());
        }
        
        public override void Down()
        {
            DropColumn("dbo.MeningoSendings", "DemisId");
            DropColumn("dbo.Sendings", "DemisId");
        }
    }
}
