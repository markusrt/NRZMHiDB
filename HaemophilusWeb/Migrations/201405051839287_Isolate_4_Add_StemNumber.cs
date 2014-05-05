namespace HaemophilusWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Isolate_4_Add_StemNumber : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Isolates", "StemNumber", c => c.Int());
            Sql("Create Unique NonClustered Index [IX_StemNumber] On [Isolates] (StemNumber) Where [StemNumber] Is Not Null");
        }

        public override void Down()
        {
            DropIndex("dbo.Isolates", "IX_StemNumber");
            DropColumn("dbo.Isolates", "StemNumber");
        }
    }
}
