namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Sending_AutoInvasive : DbMigration
    {
        public override void Up()
        {
            Sql("UPDATE dbo.Sendings SET SamplingLocation = 3 Where Invasive = 1 And SamplingLocation = 2");
            DropColumn("dbo.Sendings", "Invasive");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Sendings", "Invasive", c => c.Int());
            Sql("UPDATE dbo.Sendings SET Invasive = 1 Where SamplingLocation = 0 Or SamplingLocation = 1 Or SamplingLocation = 3");
            Sql("UPDATE dbo.Sendings SET SamplingLocation = 2 Where SamplingLocation = 3");
        }
    }
}
