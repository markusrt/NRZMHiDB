namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SendingBase_DemisId_To_String : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Sendings", "DemisId", c => c.String());
            AlterColumn("dbo.MeningoSendings", "DemisId", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.MeningoSendings", "DemisId", c => c.Guid());
            AlterColumn("dbo.Sendings", "DemisId", c => c.Guid());
        }
    }
}
