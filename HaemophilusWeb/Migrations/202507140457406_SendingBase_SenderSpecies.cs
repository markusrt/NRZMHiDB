namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SendingBase_SenderSpecies : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MeningoSendings", "SenderSpecies", c => c.String());
            RenameColumn("dbo.Sendings", "SenderConclusion", "SenderSpecies");
        }
        
        public override void Down()
        {
            DropColumn("dbo.MeningoSendings", "SenderSpecies");
            RenameColumn("dbo.Sendings", "SenderSpecies", "SenderConclusion");
        }
    }
}
